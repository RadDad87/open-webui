from __future__ import annotations

from typing import Any, Literal

from pydantic import BaseModel, ConfigDict, Field


class Location(BaseModel):
    lat: float = Field(..., ge=-90, le=90)
    lon: float = Field(..., ge=-180, le=180)


class ShapePoint(BaseModel):
    lat: float = Field(..., ge=-90, le=90)
    lon: float = Field(..., ge=-180, le=180)
    type: Literal['break', 'through', 'via'] | None = None


class RouteRequest(BaseModel):
    model_config = ConfigDict(extra='allow')

    locations: list[Location] = Field(..., min_length=2)
    costing: str = Field(default='auto')
    units: Literal['kilometers', 'miles'] = Field(default='kilometers')


class MapMatchRequest(BaseModel):
    model_config = ConfigDict(extra='allow')

    shape: list[ShapePoint] = Field(..., min_length=2)
    costing: str = Field(default='auto')
    shape_match: Literal['walk_or_snap', 'map_snap', 'edge_walk'] = Field(default='map_snap')


class Contour(BaseModel):
    time: int | None = Field(default=None, ge=1)
    distance: float | None = Field(default=None, gt=0)


class IsochroneRequest(BaseModel):
    model_config = ConfigDict(extra='allow')

    locations: list[Location] = Field(..., min_length=1)
    contours: list[Contour] = Field(..., min_length=1)
    costing: str = Field(default='auto')
    polygons: bool = Field(default=True)


class ValhallaResult(BaseModel):
    ok: bool = True
    data: dict[str, Any]


class GeoHealthResponse(BaseModel):
    ok: bool
    service: str = 'valhalla'
    base_url: str
    status_code: int | None = None
    detail: str | None = None


class Coordinate(BaseModel):
    lat: float = Field(..., ge=-90, le=90)
    lon: float = Field(..., ge=-180, le=180)


class RouteArea(BaseModel):
    model_config = ConfigDict(extra='allow')

    bbox: list[float] = Field(..., min_length=4, max_length=4)


class SiteSearchConstraints(BaseModel):
    max_slope_pct: float | None = Field(default=None, ge=0)
    water_proximity_km: float | None = Field(default=None, ge=0)
    access_mode: Literal['foot', 'vehicle', 'mixed'] | None = None


class SiteSearchRequest(BaseModel):
    model_config = ConfigDict(extra='allow')

    query_type: Literal['camp', 'farm', 'shelter']
    coordinates: Coordinate | None = None
    route_area: RouteArea | None = None
    radius_km: float = Field(default=5.0, gt=0)
    constraints: SiteSearchConstraints | None = None


class SiteResult(BaseModel):
    coordinates: Coordinate
    score: float = Field(..., ge=0, le=1)
    explanation: list[str]


class SiteSearchResponse(BaseModel):
    ok: bool = True
    query_type: Literal['camp', 'farm', 'shelter']
    results: list[SiteResult]


class AdviceRequest(BaseModel):
    model_config = ConfigDict(extra='allow')

    question: str = Field(..., min_length=3)
    coordinates: Coordinate | None = None
    destination: Coordinate | None = None
    route_area: RouteArea | None = None
    radius_km: float = Field(default=5.0, gt=0)
    constraints: SiteSearchConstraints | None = None
    costing: str = Field(default='auto')
    units: Literal['kilometers', 'miles'] = Field(default='kilometers')
    include_terrain: bool = Field(default=True)
    max_results: int = Field(default=5, ge=1, le=20)


class AdviceResponse(BaseModel):
    ok: bool = True
    intent: Literal['route', 'camp', 'farm', 'shelter']
    structured_query: dict[str, Any]
    selected_results: list[dict[str, Any]]
    explanation: str
    warnings: list[str] = Field(default_factory=list)
    assumptions: list[str] = Field(default_factory=list)
