from __future__ import annotations

import os
from pathlib import Path


def _env_flag(name: str, default: bool) -> bool:
    return os.environ.get(name, str(default)).lower() in {'1', 'true', 'yes', 'on'}


# Root directory for offline geo assets.
GEO_DATA_DIR = Path(os.environ.get('GEO_DATA_DIR', '/app/backend/data/geo')).resolve()

# DEM-related local directories.
DEM_DATA_DIR = Path(os.environ.get('DEM_DATA_DIR', str(GEO_DATA_DIR / 'dem'))).resolve()
DEM_CACHE_DIR = Path(os.environ.get('DEM_CACHE_DIR', str(GEO_DATA_DIR / 'dem_cache'))).resolve()
TERRAIN_REPORTS_DIR = Path(
    os.environ.get('TERRAIN_REPORTS_DIR', str(GEO_DATA_DIR / 'terrain_reports'))
).resolve()

# Feature flags / defaults.
ENABLE_TERRAIN_ANALYSIS = _env_flag('ENABLE_TERRAIN_ANALYSIS', True)
ENABLE_3DEP_PLACEHOLDER = _env_flag('ENABLE_3DEP_PLACEHOLDER', True)
ENABLE_REAL_ELEVATION = _env_flag('ENABLE_REAL_ELEVATION', True)
WATER_DATA_SOURCE = os.environ.get('WATER_DATA_SOURCE', 'placeholder').strip().lower()

# Placeholder defaults until real DEM integration is wired.
DEFAULT_DEM_PROVIDER = os.environ.get('DEFAULT_DEM_PROVIDER', '3dep')
DEFAULT_DEM_RESOLUTION_M = int(os.environ.get('DEFAULT_DEM_RESOLUTION_M', '10'))
DEFAULT_SLOPE_SAMPLE_DISTANCE_M = float(os.environ.get('DEFAULT_SLOPE_SAMPLE_DISTANCE_M', '30'))
DEFAULT_ROUTE_DIFFICULTY_THRESHOLDS = {
    'easy': {'max_slope_pct': 6, 'gain_m_per_km': 25},
    'moderate': {'max_slope_pct': 10, 'gain_m_per_km': 60},
    'hard': {'max_slope_pct': 16, 'gain_m_per_km': 100},
}

# Ensure local dirs exist for offline operation.
for _path in (GEO_DATA_DIR, DEM_DATA_DIR, DEM_CACHE_DIR, TERRAIN_REPORTS_DIR):
    _path.mkdir(parents=True, exist_ok=True)
