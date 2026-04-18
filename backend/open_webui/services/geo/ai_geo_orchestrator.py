from __future__ import annotations

from typing import Any, Literal

from .suitability_service import find_campsites, find_farming_areas, find_shelter_zones
from .terrain_service import (
    compute_elevation_gain_loss,
    compute_slope_profile,
    estimate_route_difficulty,
    extract_route_metrics,
    summarize_terrain_risks,
)
from .valhalla_client import ValhallaClient

Intent = Literal['route', 'camp', 'farm', 'shelter']


def classify_geo_intent(question: str) -> Intent:
    text = question.lower()

    route_terms = {
        'route', 'directions', 'navigate', 'navigation', 'drive', 'walk', 'hike', 'bike', 'path', 'trip to', 'get to'
    }
    camp_terms = {'camp', 'campsite', 'tent', 'camping'}
    farm_terms = {'farm', 'farming', 'agriculture', 'cultivate', 'crop', 'soil'}
    shelter_terms = {'survival shelter', 'lawful shelter', 'emergency shelter', 'shelter site', 'shelter'}

    if any(term in text for term in route_terms):
        return 'route'
    if any(term in text for term in camp_terms):
        return 'camp'
    if any(term in text for term in farm_terms):
        return 'farm'
    if any(term in text for term in shelter_terms):
        return 'shelter'

    return 'route'


class AIGeoOrchestrator:
    def __init__(self, client: ValhallaClient | None = None):
        self.client = client or ValhallaClient()

    async def advise(self, request: dict[str, Any]) -> dict[str, Any]:
        question = str(request.get('question') or '').strip()
        if not question:
            raise ValueError('question is required')

        intent = classify_geo_intent(question)
        constraints = request.get('constraints') or {}
        warnings: list[str] = []
        assumptions: list[str] = []

        if intent == 'route':
            result = await self._advise_route(request, warnings, assumptions)
        elif intent == 'camp':
            result = self._advise_site('camp', request, constraints, warnings, assumptions)
        elif intent == 'farm':
            result = self._advise_site('farm', request, constraints, warnings, assumptions)
        else:
            result = self._advise_site('shelter', request, constraints, warnings, assumptions)
            warnings.append('shelter guidance is limited to lawful survival shelter siting and safety-oriented criteria')

        return {
            'ok': True,
            'intent': intent,
            **result,
            'warnings': warnings,
            'assumptions': assumptions,
        }

    async def _advise_route(
        self,
        request: dict[str, Any],
        warnings: list[str],
        assumptions: list[str],
    ) -> dict[str, Any]:
        origin = request.get('coordinates')
        destination = request.get('destination')

        if not origin or not destination:
            raise ValueError('route intent requires both coordinates and destination')

        payload = {
            'locations': [origin, destination],
            'costing': request.get('costing', 'auto'),
            'units': request.get('units', 'kilometers'),
        }

        route_data = await self.client.route(payload)
        selected_results: list[dict[str, Any]] = [{'route': route_data}]
        include_terrain = bool(request.get('include_terrain', True))

        explanation = 'Computed route via Valhalla offline endpoint.'
        if include_terrain:
            metrics = extract_route_metrics(route_data)
            slope_profile = compute_slope_profile(metrics['elevations_m'], sample_distance_m=float(metrics.get('sample_distance_m', 30.0)))
            elevation = compute_elevation_gain_loss(metrics['elevations_m'])
            difficulty = estimate_route_difficulty(
                distance_km=metrics['distance_km'],
                elevation_gain_m=elevation['elevation_gain_m'],
                max_slope_pct=slope_profile['max_slope_pct'],
            )
            risks = summarize_terrain_risks(
                max_slope_pct=slope_profile['max_slope_pct'],
                elevation_gain_m=elevation['elevation_gain_m'],
                distance_km=metrics['distance_km'],
                elevation_samples_available=bool(metrics['elevations_m']),
            )
            selected_results[0]['terrain'] = {
                'distance_km': metrics['distance_km'],
                'slope_profile': slope_profile,
                **elevation,
                'difficulty': difficulty,
                'risks': risks,
            }
            explanation = (
                f"Computed route and terrain summary: difficulty={difficulty}, "
                f"max_slope={slope_profile['max_slope_pct']:.2f}%, "
                f"elevation_gain={elevation['elevation_gain_m']:.2f}m."
            )
            if not metrics['elevations_m']:
                warnings.append(f"terrain analysis used {metrics.get('elevation_source', 'placeholder')} elevation inputs")

        structured_query = {
            'tool': 'valhalla_client.route',
            'payload': payload,
            'include_terrain': include_terrain,
        }
        assumptions.append('routing depends on local Valhalla data freshness and configured costing profile')

        return {
            'structured_query': structured_query,
            'selected_results': selected_results,
            'explanation': explanation,
        }

    def _advise_site(
        self,
        intent: Literal['camp', 'farm', 'shelter'],
        request: dict[str, Any],
        constraints: dict[str, Any],
        warnings: list[str],
        assumptions: list[str],
    ) -> dict[str, Any]:
        coordinates = request.get('coordinates')
        route_area = request.get('route_area')
        radius_km = float(request.get('radius_km', 5.0))
        max_results = int(request.get('max_results', 5))

        if not coordinates and not route_area:
            raise ValueError(f'{intent} intent requires coordinates or route_area')

        tool_name: str
        if intent == 'camp':
            ranked = find_campsites(coordinates=coordinates, route_area=route_area, radius_km=radius_km, constraints=constraints)
            tool_name = 'suitability_service.find_campsites'
        elif intent == 'farm':
            ranked = find_farming_areas(coordinates=coordinates, route_area=route_area, radius_km=radius_km, constraints=constraints)
            tool_name = 'suitability_service.find_farming_areas'
        else:
            ranked = find_shelter_zones(coordinates=coordinates, route_area=route_area, radius_km=radius_km, constraints=constraints)
            tool_name = 'suitability_service.find_shelter_zones'
            warnings.append('do not use this output for trespass, concealment, or unlawful activity')

        selected_results = ranked[:max_results]
        top_score = selected_results[0]['score'] if selected_results else 0.0

        structured_query = {
            'tool': tool_name,
            'payload': {
                'coordinates': coordinates,
                'route_area': route_area,
                'radius_km': radius_km,
                'constraints': constraints,
            },
        }

        assumptions.append('suitability scoring currently uses offline placeholder terrain and land/water proxies')
        explanation = (
            f"Ranked {intent} candidates using terrain slope, elevation effort, and proximity proxies. "
            f"Top score is {top_score:.2f}."
        )

        return {
            'structured_query': structured_query,
            'selected_results': selected_results,
            'explanation': explanation,
        }
