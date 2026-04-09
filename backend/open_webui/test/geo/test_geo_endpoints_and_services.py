from __future__ import annotations

import pytest

fastapi = pytest.importorskip('fastapi')
pytest.importorskip('httpx')
from fastapi import FastAPI
from fastapi.testclient import TestClient

from open_webui.routers import geo as geo_router
from open_webui.services.geo.ai_geo_orchestrator import classify_geo_intent
from open_webui.services.geo.suitability_service import (
    find_campsites,
    find_farming_areas,
    find_shelter_zones,
)
from open_webui.services.geo.terrain_service import sample_elevations_for_coordinates
from open_webui.utils.auth import get_verified_user


class _FakeValhallaClient:
    async def route(self, payload):
        return {
            'trip': {
                'summary': {'length': 3.2},
                'legs': [
                    {
                        'summary': {'length': 3.2},
                        'shape': '_p~iF~ps|U_ulLnnqC_mqNvxq`@',
                    }
                ],
            }
        }

    async def map_match(self, payload):
        return {'ok': True, 'payload': payload}

    async def isochrone(self, payload):
        return {'ok': True, 'payload': payload}

    async def health_check(self):
        return True, 200, None


def _build_client(monkeypatch) -> TestClient:
    app = FastAPI()
    monkeypatch.setattr(geo_router, 'client', _FakeValhallaClient())
    monkeypatch.setattr(geo_router.orchestrator, 'client', _FakeValhallaClient())
    app.include_router(geo_router.router, prefix='/api/geo')
    app.dependency_overrides[get_verified_user] = lambda: {'id': 'test-user'}
    return TestClient(app)


def test_geo_health_route_site_and_advice_endpoints(monkeypatch):
    client = _build_client(monkeypatch)

    health = client.get('/api/geo/health')
    assert health.status_code == 200
    assert health.json()['ok'] is True

    route = client.post(
        '/api/geo/route?include_terrain=true',
        json={
            'locations': [{'lat': 47.6, 'lon': -122.3}, {'lat': 47.61, 'lon': -122.31}],
            'costing': 'auto',
            'units': 'kilometers',
        },
    )
    assert route.status_code == 200
    assert route.json()['ok'] is True

    sites = client.post(
        '/api/geo/site-search',
        json={
            'query_type': 'camp',
            'coordinates': {'lat': 47.6, 'lon': -122.3},
            'radius_km': 5,
        },
    )
    assert sites.status_code == 200
    assert sites.json()['query_type'] == 'camp'
    assert len(sites.json()['results']) > 0

    advice = client.post(
        '/api/geo/advice',
        json={
            'question': 'Find a camp spot nearby',
            'coordinates': {'lat': 47.6, 'lon': -122.3},
            'radius_km': 5,
        },
    )
    assert advice.status_code == 200
    assert advice.json()['intent'] in {'route', 'camp', 'farm', 'shelter'}


def test_terrain_fallback_behavior_dem_present_and_missing(monkeypatch):
    coords = [(47.6, -122.3), (47.61, -122.31)]

    monkeypatch.setattr('open_webui.services.geo.terrain_service.dem_loader.available', lambda: True)
    monkeypatch.setattr('open_webui.services.geo.terrain_service.dem_loader.sample_profile', lambda _: [100.0, 110.0])
    elev, source = sample_elevations_for_coordinates(coords)
    assert source == 'dem'
    assert elev == [100.0, 110.0]

    monkeypatch.setattr('open_webui.services.geo.terrain_service.dem_loader.available', lambda: True)
    monkeypatch.setattr('open_webui.services.geo.terrain_service.dem_loader.sample_profile', lambda _: None)
    elev2, source2 = sample_elevations_for_coordinates(coords)
    assert source2 == 'placeholder'
    assert len(elev2) == 2


def test_suitability_scoring_camp_farm_shelter():
    camp = find_campsites(coordinates={'lat': 47.6, 'lon': -122.3}, radius_km=5)
    farm = find_farming_areas(coordinates={'lat': 47.6, 'lon': -122.3}, radius_km=5)
    shelter = find_shelter_zones(coordinates={'lat': 47.6, 'lon': -122.3}, radius_km=5)

    assert len(camp) > 0 and len(farm) > 0 and len(shelter) > 0
    assert camp[0]['score'] >= camp[-1]['score']
    assert farm[0]['score'] >= farm[-1]['score']
    assert shelter[0]['score'] >= shelter[-1]['score']


def test_ai_orchestration_intent_classification():
    assert classify_geo_intent('Give me a route from A to B') == 'route'
    assert classify_geo_intent('Find me a camp location near water') == 'camp'
    assert classify_geo_intent('Any good farming terrain nearby?') == 'farm'
    assert classify_geo_intent('Need a lawful survival shelter site') == 'shelter'
