from __future__ import annotations

import math
from typing import Any

from .config import DEFAULT_ROUTE_DIFFICULTY_THRESHOLDS, DEFAULT_SLOPE_SAMPLE_DISTANCE_M
from .dem_loader import dem_loader


def _haversine_distance_m(lat1: float, lon1: float, lat2: float, lon2: float) -> float:
    r = 6_371_000.0
    phi1 = math.radians(lat1)
    phi2 = math.radians(lat2)
    dphi = math.radians(lat2 - lat1)
    dlambda = math.radians(lon2 - lon1)
    a = math.sin(dphi / 2) ** 2 + math.cos(phi1) * math.cos(phi2) * math.sin(dlambda / 2) ** 2
    return 2 * r * math.atan2(math.sqrt(a), math.sqrt(1 - a))


def _placeholder_elevation(lat: float, lon: float) -> float:
    return 220 + 80 * math.sin(math.radians(lat * 3.0)) + 65 * math.cos(math.radians(lon * 2.0))


def sample_elevations_for_coordinates(
    coordinates: list[tuple[float, float]],
) -> tuple[list[float], str]:
    if coordinates and dem_loader.available():
        profile = dem_loader.sample_profile(coordinates)
        if profile is not None:
            return profile, 'dem'

    return [_placeholder_elevation(lat, lon) for lat, lon in coordinates], 'placeholder'


def compute_slope_profile(
    elevations_m: list[float],
    sample_distance_m: float = DEFAULT_SLOPE_SAMPLE_DISTANCE_M,
) -> dict[str, Any]:
    if len(elevations_m) < 2 or sample_distance_m <= 0:
        return {
            'samples': len(elevations_m),
            'sample_distance_m': sample_distance_m,
            'slopes_pct': [],
            'max_slope_pct': 0.0,
            'min_slope_pct': 0.0,
            'avg_abs_slope_pct': 0.0,
        }

    slopes_pct: list[float] = []
    for prev_elev, curr_elev in zip(elevations_m[:-1], elevations_m[1:]):
        rise = curr_elev - prev_elev
        slopes_pct.append((rise / sample_distance_m) * 100.0)

    abs_slopes = [abs(s) for s in slopes_pct]
    return {
        'samples': len(elevations_m),
        'sample_distance_m': sample_distance_m,
        'slopes_pct': slopes_pct,
        'max_slope_pct': max(abs_slopes) if abs_slopes else 0.0,
        'min_slope_pct': min(slopes_pct) if slopes_pct else 0.0,
        'avg_abs_slope_pct': sum(abs_slopes) / len(abs_slopes) if abs_slopes else 0.0,
    }


def compute_elevation_gain_loss(elevations_m: list[float]) -> dict[str, float]:
    gain = 0.0
    loss = 0.0

    for prev_elev, curr_elev in zip(elevations_m[:-1], elevations_m[1:]):
        delta = curr_elev - prev_elev
        if delta > 0:
            gain += delta
        elif delta < 0:
            loss += abs(delta)

    return {
        'elevation_gain_m': round(gain, 2),
        'elevation_loss_m': round(loss, 2),
        'elevation_net_m': round(gain - loss, 2),
    }


def estimate_route_difficulty(
    distance_km: float,
    elevation_gain_m: float,
    max_slope_pct: float,
) -> str:
    if distance_km <= 0:
        distance_km = 1e-6

    gain_per_km = elevation_gain_m / distance_km
    easy = DEFAULT_ROUTE_DIFFICULTY_THRESHOLDS['easy']
    moderate = DEFAULT_ROUTE_DIFFICULTY_THRESHOLDS['moderate']
    hard = DEFAULT_ROUTE_DIFFICULTY_THRESHOLDS['hard']

    if max_slope_pct <= easy['max_slope_pct'] and gain_per_km <= easy['gain_m_per_km']:
        return 'easy'
    if max_slope_pct <= moderate['max_slope_pct'] and gain_per_km <= moderate['gain_m_per_km']:
        return 'moderate'
    if max_slope_pct <= hard['max_slope_pct'] and gain_per_km <= hard['gain_m_per_km']:
        return 'hard'
    return 'extreme'


def summarize_terrain_risks(
    max_slope_pct: float,
    elevation_gain_m: float,
    distance_km: float,
    elevation_samples_available: bool,
) -> list[str]:
    risks: list[str] = []

    if not elevation_samples_available:
        risks.append('elevation-data-unavailable-using-placeholder-analysis')

    if max_slope_pct >= 18:
        risks.append('very-steep-grade-segments')
    elif max_slope_pct >= 12:
        risks.append('steep-grade-segments')

    if distance_km > 0 and (elevation_gain_m / distance_km) >= 120:
        risks.append('high-climb-intensity')

    if not risks:
        risks.append('no-major-terrain-risks-detected-with-current-data')

    return risks


def _decode_polyline6(encoded: str) -> list[tuple[float, float]]:
    coords: list[tuple[float, float]] = []
    idx = 0
    lat = 0
    lon = 0

    while idx < len(encoded):
        shift = 0
        result = 0
        while True:
            b = ord(encoded[idx]) - 63
            idx += 1
            result |= (b & 0x1F) << shift
            shift += 5
            if b < 0x20:
                break
        dlat = ~(result >> 1) if result & 1 else (result >> 1)
        lat += dlat

        shift = 0
        result = 0
        while True:
            b = ord(encoded[idx]) - 63
            idx += 1
            result |= (b & 0x1F) << shift
            shift += 5
            if b < 0x20:
                break
        dlon = ~(result >> 1) if result & 1 else (result >> 1)
        lon += dlon

        coords.append((lat / 1_000_000.0, lon / 1_000_000.0))

    return coords


def _extract_route_coordinates(valhalla_route_response: dict[str, Any]) -> list[tuple[float, float]]:
    trip = valhalla_route_response.get('trip', {})
    legs = trip.get('legs', [])

    explicit = valhalla_route_response.get('coordinates') or trip.get('coordinates')
    if isinstance(explicit, list):
        points = []
        for p in explicit:
            if isinstance(p, dict) and 'lat' in p and 'lon' in p:
                points.append((float(p['lat']), float(p['lon'])))
            elif isinstance(p, (list, tuple)) and len(p) >= 2:
                points.append((float(p[0]), float(p[1])))
        if points:
            return points

    points: list[tuple[float, float]] = []
    for leg in legs:
        shape = leg.get('shape')
        if isinstance(shape, str) and shape:
            try:
                points.extend(_decode_polyline6(shape))
            except Exception:
                continue

    return points


def extract_route_metrics(valhalla_route_response: dict[str, Any]) -> dict[str, Any]:
    trip = valhalla_route_response.get('trip', {})
    legs = trip.get('legs', [])
    summary = trip.get('summary', {})

    distance_km = float(summary.get('length') or 0.0)
    if distance_km <= 0 and legs:
        distance_km = float(sum((leg.get('summary', {}) or {}).get('length', 0.0) for leg in legs))

    elevations = valhalla_route_response.get('elevations_m') or trip.get('elevations_m') or []
    numeric = [float(v) for v in elevations if isinstance(v, (int, float))]

    source = 'route-response'
    if not numeric:
        coords = _extract_route_coordinates(valhalla_route_response)
        if len(coords) >= 2:
            numeric, source = sample_elevations_for_coordinates(coords)
        else:
            numeric = []
            source = 'unavailable'

    sample_distance_m = DEFAULT_SLOPE_SAMPLE_DISTANCE_M
    coords_for_distance = _extract_route_coordinates(valhalla_route_response)
    if len(coords_for_distance) >= 2:
        segment_distances = [
            _haversine_distance_m(a[0], a[1], b[0], b[1])
            for a, b in zip(coords_for_distance[:-1], coords_for_distance[1:])
        ]
        if segment_distances:
            sample_distance_m = sum(segment_distances) / len(segment_distances)

    return {
        'distance_km': round(distance_km, 3),
        'elevations_m': numeric,
        'elevation_source': source,
        'sample_distance_m': round(sample_distance_m, 2),
    }
