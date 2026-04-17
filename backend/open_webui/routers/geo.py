from __future__ import annotations

import httpx
from fastapi import APIRouter, Depends, HTTPException, Query

from open_webui.models.users import UserModel
from open_webui.services.geo.config import ENABLE_TERRAIN_ANALYSIS
from open_webui.services.geo.schemas import (
    AdviceRequest,
    AdviceResponse,
    GeoHealthResponse,
    IsochroneRequest,
    MapMatchRequest,
    RouteRequest,
    SiteResult,
    SiteSearchRequest,
    SiteSearchResponse,
    ValhallaResult,
)
from open_webui.services.geo.ai_geo_orchestrator import AIGeoOrchestrator
from open_webui.services.geo.suitability_service import (
    find_campsites,
    find_farming_areas,
    find_shelter_zones,
)
from open_webui.services.geo.terrain_service import (
    compute_elevation_gain_loss,
    compute_slope_profile,
    estimate_route_difficulty,
    extract_route_metrics,
    summarize_terrain_risks,
)
from open_webui.services.geo.valhalla_client import ValhallaClient
from open_webui.utils.auth import get_verified_user

router = APIRouter()
client = ValhallaClient()
orchestrator = AIGeoOrchestrator(client)


@router.post('/route', response_model=ValhallaResult)
async def route(
    form_data: RouteRequest,
    include_terrain: bool = Query(default=False),
    user: UserModel = Depends(get_verified_user),
):
    try:
        data = await client.route(form_data.model_dump(exclude_none=True))
        if include_terrain and ENABLE_TERRAIN_ANALYSIS:
            metrics = extract_route_metrics(data)
            slope_profile = compute_slope_profile(metrics['elevations_m'])
            elev = compute_elevation_gain_loss(metrics['elevations_m'])
            difficulty = estimate_route_difficulty(
                distance_km=metrics['distance_km'],
                elevation_gain_m=elev['elevation_gain_m'],
                max_slope_pct=slope_profile['max_slope_pct'],
            )
            risks = summarize_terrain_risks(
                max_slope_pct=slope_profile['max_slope_pct'],
                elevation_gain_m=elev['elevation_gain_m'],
                distance_km=metrics['distance_km'],
                elevation_samples_available=bool(metrics['elevations_m']),
            )
            data['terrain'] = {
                'distance_km': metrics['distance_km'],
                **elev,
                'slope_profile': slope_profile,
                'difficulty': difficulty,
                'risks': risks,
            }
        return ValhallaResult(ok=True, data=data)
    except httpx.HTTPStatusError as exc:
        raise HTTPException(status_code=exc.response.status_code, detail=exc.response.text) from exc
    except Exception as exc:
        raise HTTPException(status_code=502, detail=str(exc)) from exc


@router.post('/map-match', response_model=ValhallaResult)
async def map_match(
    form_data: MapMatchRequest,
    user: UserModel = Depends(get_verified_user),
):
    try:
        data = await client.map_match(form_data.model_dump(exclude_none=True))
        return ValhallaResult(ok=True, data=data)
    except httpx.HTTPStatusError as exc:
        raise HTTPException(status_code=exc.response.status_code, detail=exc.response.text) from exc
    except Exception as exc:
        raise HTTPException(status_code=502, detail=str(exc)) from exc


@router.post('/isochrone', response_model=ValhallaResult)
async def isochrone(
    form_data: IsochroneRequest,
    user: UserModel = Depends(get_verified_user),
):
    try:
        data = await client.isochrone(form_data.model_dump(exclude_none=True))
        return ValhallaResult(ok=True, data=data)
    except httpx.HTTPStatusError as exc:
        raise HTTPException(status_code=exc.response.status_code, detail=exc.response.text) from exc
    except Exception as exc:
        raise HTTPException(status_code=502, detail=str(exc)) from exc


@router.get('/health', response_model=GeoHealthResponse)
async def health() -> GeoHealthResponse:
    ok, status_code, detail = await client.health_check()
    return GeoHealthResponse(
        ok=ok,
        base_url=client.base_url,
        status_code=status_code,
        detail=detail,
    )


@router.post('/site-search', response_model=SiteSearchResponse)
async def site_search(
    form_data: SiteSearchRequest,
    user: UserModel = Depends(get_verified_user),
) -> SiteSearchResponse:
    query_type = form_data.query_type
    coordinates = form_data.coordinates.model_dump() if form_data.coordinates else None
    route_area = form_data.route_area.model_dump() if form_data.route_area else None
    constraints = form_data.constraints.model_dump(exclude_none=True) if form_data.constraints else None

    if query_type == 'camp':
        ranked = find_campsites(
            coordinates=coordinates,
            route_area=route_area,
            radius_km=form_data.radius_km,
            constraints=constraints,
        )
    elif query_type == 'farm':
        ranked = find_farming_areas(
            coordinates=coordinates,
            route_area=route_area,
            radius_km=form_data.radius_km,
            constraints=constraints,
        )
    elif query_type == 'shelter':
        ranked = find_shelter_zones(
            coordinates=coordinates,
            route_area=route_area,
            radius_km=form_data.radius_km,
            constraints=constraints,
        )
    else:
        raise HTTPException(status_code=400, detail=f'Unsupported query_type: {query_type}')

    return SiteSearchResponse(
        ok=True,
        query_type=query_type,
        results=[SiteResult(**item) for item in ranked],
    )


@router.post('/advice', response_model=AdviceResponse)
async def geo_advice(
    form_data: AdviceRequest,
    user: UserModel = Depends(get_verified_user),
) -> AdviceResponse:
    try:
        result = await orchestrator.advise(form_data.model_dump(exclude_none=True))
        return AdviceResponse(**result)
    except ValueError as exc:
        raise HTTPException(status_code=400, detail=str(exc)) from exc
    except httpx.HTTPStatusError as exc:
        raise HTTPException(status_code=exc.response.status_code, detail=exc.response.text) from exc
    except Exception as exc:
        raise HTTPException(status_code=502, detail=str(exc)) from exc
