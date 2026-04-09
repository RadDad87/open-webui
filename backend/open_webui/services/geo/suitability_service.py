from __future__ import annotations

import json
import math
from functools import lru_cache
from pathlib import Path
from typing import Any

from .config import GEO_DATA_DIR, WATER_DATA_SOURCE
from .terrain_service import (
    compute_elevation_gain_loss,
    compute_slope_profile,
    sample_elevations_for_coordinates,
)


def _clamp01(value: float) -> float:
    return max(0.0, min(1.0, value))


def _haversine_km(lat1: float, lon1: float, lat2: float, lon2: float) -> float:
    r = 6371.0
    p1 = math.radians(lat1)
    p2 = math.radians(lat2)
    dp = math.radians(lat2 - lat1)
    dl = math.radians(lon2 - lon1)
    a = math.sin(dp / 2) ** 2 + math.cos(p1) * math.cos(p2) * math.sin(dl / 2) ** 2
    return 2 * r * math.atan2(math.sqrt(a), math.sqrt(1 - a))


def _centroid_from_route_area(route_area: dict[str, Any]) -> tuple[float, float]:
    bbox = route_area.get('bbox', [])
    if not isinstance(bbox, list) or len(bbox) != 4:
        raise ValueError('route_area.bbox must contain [min_lon, min_lat, max_lon, max_lat]')

    min_lon, min_lat, max_lon, max_lat = bbox
    return ((float(min_lat) + float(max_lat)) / 2.0, (float(min_lon) + float(max_lon)) / 2.0)


def _resolve_center(coordinates: dict[str, Any] | None, route_area: dict[str, Any] | None) -> tuple[float, float]:
    if coordinates:
        return float(coordinates['lat']), float(coordinates['lon'])
    if route_area:
        return _centroid_from_route_area(route_area)
    raise ValueError('either coordinates or route_area is required')


def _generate_candidates(center_lat: float, center_lon: float, radius_km: float) -> list[tuple[float, float]]:
    offsets = [
        (0.0, 0.0),
        (0.4, 0.0),
        (-0.4, 0.0),
        (0.0, 0.4),
        (0.0, -0.4),
        (0.28, 0.28),
        (-0.28, 0.28),
        (0.28, -0.28),
        (-0.28, -0.28),
    ]

    deg_radius = radius_km / 111.0
    return [(center_lat + d_lat * deg_radius, center_lon + d_lon * deg_radius) for d_lat, d_lon in offsets]


def _terrain_metrics(lat: float, lon: float) -> dict[str, Any]:
    points = [
        (lat, lon),
        (lat + 0.0008, lon),
        (lat - 0.0008, lon),
        (lat, lon + 0.0008),
        (lat, lon - 0.0008),
    ]
    elevations, source = sample_elevations_for_coordinates(points)
    slope = compute_slope_profile([float(v) for v in elevations], sample_distance_m=30.0)
    elev = compute_elevation_gain_loss([float(v) for v in elevations])
    return {
        'max_slope_pct': float(slope['max_slope_pct']),
        'elevation_gain_m': float(elev['elevation_gain_m']),
        'source': source,
    }


@lru_cache(maxsize=1)
def _load_local_water_points() -> list[tuple[float, float]]:
    water_dir = Path(GEO_DATA_DIR) / 'water'
    if not water_dir.exists():
        return []

    points: list[tuple[float, float]] = []
    for path in water_dir.glob('*.geojson'):
        try:
            payload = json.loads(path.read_text(encoding='utf-8'))
        except (OSError, json.JSONDecodeError):
            continue

        for feature in payload.get('features', []):
            props = feature.get('properties', {}) or {}
            is_water = (
                props.get('natural') == 'water'
                or 'waterway' in props
                or props.get('landuse') == 'reservoir'
                or props.get('water') is not None
            )
            if not is_water:
                continue

            geom = feature.get('geometry', {}) or {}
            gtype = geom.get('type')
            coords = geom.get('coordinates')
            if gtype == 'Point' and isinstance(coords, list) and len(coords) >= 2:
                points.append((float(coords[1]), float(coords[0])))
            elif gtype == 'LineString' and isinstance(coords, list) and coords:
                c0 = coords[0]
                if isinstance(c0, list) and len(c0) >= 2:
                    points.append((float(c0[1]), float(c0[0])))
            elif gtype == 'Polygon' and isinstance(coords, list) and coords and isinstance(coords[0], list) and coords[0]:
                c0 = coords[0][0]
                if isinstance(c0, list) and len(c0) >= 2:
                    points.append((float(c0[1]), float(c0[0])))

    return points


def _water_proximity_score(lat: float, lon: float) -> tuple[float, str]:
    source = WATER_DATA_SOURCE
    if source in {'local', 'osm'}:
        points = _load_local_water_points()
        if points:
            nearest_km = min(_haversine_km(lat, lon, p_lat, p_lon) for p_lat, p_lon in points)
            score = _clamp01(1.0 - (nearest_km / 5.0))
            return score, f'{source}-tags'

    score = _clamp01(0.5 + 0.5 * math.sin(math.radians((lat + lon) * 4.0)))
    return score, 'placeholder'


def _land_use_placeholder(lat: float, lon: float, query_type: str) -> float:
    seed = 0.5 + 0.5 * math.cos(math.radians((lat * 5.0) - (lon * 3.0)))
    if query_type == 'farm':
        return _clamp01(seed * 0.9 + 0.1)
    if query_type == 'shelter':
        return _clamp01(1.0 - abs(seed - 0.6))
    return _clamp01(seed)


def _access_placeholder(access_mode: str | None) -> float:
    if access_mode == 'vehicle':
        return 0.7
    if access_mode == 'foot':
        return 0.85
    if access_mode == 'mixed':
        return 0.8
    return 0.75


def _score_candidate(
    query_type: str,
    lat: float,
    lon: float,
    constraints: dict[str, Any] | None,
) -> dict[str, Any]:
    constraints = constraints or {}

    terrain = _terrain_metrics(lat, lon)
    water_score, water_source = _water_proximity_score(lat, lon)
    land_use_score = _land_use_placeholder(lat, lon, query_type)
    access_score = _access_placeholder(constraints.get('access_mode'))

    max_slope = terrain['max_slope_pct']
    slope_limit = constraints.get('max_slope_pct')
    slope_penalty = 0.0
    if slope_limit is not None and max_slope > float(slope_limit):
        slope_penalty = min(0.4, (max_slope - float(slope_limit)) / 50.0)

    water_pref_km = constraints.get('water_proximity_km')
    water_penalty = 0.0
    if water_pref_km is not None:
        target = _clamp01(1.0 - (float(water_pref_km) / 10.0))
        water_penalty = max(0.0, target - water_score) * 0.25

    if query_type == 'camp':
        raw_score = (0.40 * (1.0 - min(max_slope / 25.0, 1.0))) + (0.30 * water_score) + (0.20 * land_use_score) + (0.10 * access_score)
    elif query_type == 'farm':
        raw_score = (0.45 * (1.0 - min(max_slope / 18.0, 1.0))) + (0.25 * land_use_score) + (0.20 * water_score) + (0.10 * access_score)
    else:  # shelter
        raw_score = (0.35 * (1.0 - min(max_slope / 22.0, 1.0))) + (0.35 * land_use_score) + (0.15 * access_score) + (0.15 * (1.0 - min(terrain['elevation_gain_m'] / 120.0, 1.0)))

    score = _clamp01(raw_score - slope_penalty - water_penalty)

    explanation = [
        f"terrain max slope {max_slope:.2f}% used in score ({terrain['source']})",
        f'water proximity score {water_score:.2f} ({water_source})',
        f'land-use suitability score {land_use_score:.2f} (placeholder)',
        f'access-mode factor {access_score:.2f}',
    ]
    if slope_penalty > 0:
        explanation.append(f'slope constraint penalty applied ({slope_penalty:.2f})')
    if water_penalty > 0:
        explanation.append(f'water proximity penalty applied ({water_penalty:.2f})')

    return {
        'coordinates': {'lat': round(lat, 6), 'lon': round(lon, 6)},
        'score': round(score, 4),
        'explanation': explanation,
    }


def _rank_sites(
    query_type: str,
    coordinates: dict[str, Any] | None,
    route_area: dict[str, Any] | None,
    radius_km: float,
    constraints: dict[str, Any] | None = None,
) -> list[dict[str, Any]]:
    center_lat, center_lon = _resolve_center(coordinates, route_area)
    candidates = _generate_candidates(center_lat, center_lon, radius_km)

    scored = [_score_candidate(query_type, lat, lon, constraints) for lat, lon in candidates]
    scored.sort(key=lambda item: item['score'], reverse=True)
    return scored


def find_campsites(
    coordinates: dict[str, Any] | None = None,
    route_area: dict[str, Any] | None = None,
    radius_km: float = 5.0,
    constraints: dict[str, Any] | None = None,
) -> list[dict[str, Any]]:
    return _rank_sites('camp', coordinates, route_area, radius_km, constraints)


def find_farming_areas(
    coordinates: dict[str, Any] | None = None,
    route_area: dict[str, Any] | None = None,
    radius_km: float = 5.0,
    constraints: dict[str, Any] | None = None,
) -> list[dict[str, Any]]:
    return _rank_sites('farm', coordinates, route_area, radius_km, constraints)


def find_shelter_zones(
    coordinates: dict[str, Any] | None = None,
    route_area: dict[str, Any] | None = None,
    radius_km: float = 5.0,
    constraints: dict[str, Any] | None = None,
) -> list[dict[str, Any]]:
    return _rank_sites('shelter', coordinates, route_area, radius_km, constraints)
