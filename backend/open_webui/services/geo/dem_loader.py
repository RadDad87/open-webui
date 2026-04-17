from __future__ import annotations

import math
import re
from dataclasses import dataclass
from pathlib import Path
from typing import BinaryIO

from .config import DEM_DATA_DIR, ENABLE_REAL_ELEVATION


@dataclass
class _HGTSpec:
    path: Path
    lat_min: int
    lon_min: int
    size: int


@dataclass
class _ASCSpec:
    path: Path
    ncols: int
    nrows: int
    xll: float
    yll: float
    cellsize: float
    nodata: float

    @property
    def lon_min(self) -> float:
        return self.xll

    @property
    def lon_max(self) -> float:
        return self.xll + (self.ncols * self.cellsize)

    @property
    def lat_min(self) -> float:
        return self.yll

    @property
    def lat_max(self) -> float:
        return self.yll + (self.nrows * self.cellsize)


class DEMLoader:
    """File-based DEM sampler for local SRTM (.hgt) and ASCII grid (.asc/.grd) tiles."""

    _hgt_re = re.compile(r'([NS])(\d{2})([EW])(\d{3})\.hgt$', re.IGNORECASE)

    def __init__(self, data_dir: Path | None = None):
        self.data_dir = (data_dir or DEM_DATA_DIR).resolve()
        self._hgt_tiles: list[_HGTSpec] = []
        self._asc_tiles: list[_ASCSpec] = []
        self._asc_cache: dict[Path, list[list[float]]] = {}
        self._indexed = False

    def _index_tiles(self) -> None:
        if self._indexed:
            return

        if not self.data_dir.exists():
            self._indexed = True
            return

        for path in self.data_dir.rglob('*'):
            if not path.is_file():
                continue
            suffix = path.suffix.lower()
            if suffix == '.hgt':
                spec = self._parse_hgt(path)
                if spec:
                    self._hgt_tiles.append(spec)
            elif suffix in {'.asc', '.grd'}:
                spec = self._parse_asc_header(path)
                if spec:
                    self._asc_tiles.append(spec)

        self._indexed = True

    def available(self) -> bool:
        if not ENABLE_REAL_ELEVATION:
            return False
        self._index_tiles()
        return bool(self._hgt_tiles or self._asc_tiles)

    def sample_elevation(self, lat: float, lon: float) -> float | None:
        if not ENABLE_REAL_ELEVATION:
            return None

        self._index_tiles()

        for spec in self._hgt_tiles:
            if spec.lat_min <= lat < spec.lat_min + 1 and spec.lon_min <= lon < spec.lon_min + 1:
                val = self._sample_hgt(spec, lat, lon)
                if val is not None:
                    return val

        for spec in self._asc_tiles:
            if spec.lat_min <= lat <= spec.lat_max and spec.lon_min <= lon <= spec.lon_max:
                val = self._sample_asc(spec, lat, lon)
                if val is not None:
                    return val

        return None

    def sample_profile(self, coordinates: list[tuple[float, float]]) -> list[float] | None:
        sampled: list[float] = []
        for lat, lon in coordinates:
            val = self.sample_elevation(lat, lon)
            if val is None:
                return None
            sampled.append(float(val))
        return sampled

    def _parse_hgt(self, path: Path) -> _HGTSpec | None:
        m = self._hgt_re.search(path.name)
        if not m:
            return None
        ns, lat_deg, ew, lon_deg = m.groups()
        lat_min = int(lat_deg) * (1 if ns.upper() == 'N' else -1)
        lon_min = int(lon_deg) * (1 if ew.upper() == 'E' else -1)
        size = int(math.sqrt(path.stat().st_size // 2))
        if size <= 1:
            return None
        return _HGTSpec(path=path, lat_min=lat_min, lon_min=lon_min, size=size)

    def _parse_asc_header(self, path: Path) -> _ASCSpec | None:
        header: dict[str, str] = {}
        try:
            with path.open('r', encoding='utf-8', errors='ignore') as f:
                for _ in range(6):
                    line = f.readline()
                    if not line:
                        break
                    parts = line.strip().split()
                    if len(parts) >= 2:
                        header[parts[0].lower()] = parts[-1]
        except OSError:
            return None

        required = {'ncols', 'nrows', 'cellsize'}
        if not required.issubset(header.keys()):
            return None

        xll = float(header.get('xllcorner', header.get('xllcenter', '0')))
        yll = float(header.get('yllcorner', header.get('yllcenter', '0')))
        return _ASCSpec(
            path=path,
            ncols=int(float(header['ncols'])),
            nrows=int(float(header['nrows'])),
            xll=xll,
            yll=yll,
            cellsize=float(header['cellsize']),
            nodata=float(header.get('nodata_value', '-9999')),
        )

    def _sample_hgt(self, spec: _HGTSpec, lat: float, lon: float) -> float | None:
        arc = spec.size - 1
        lat_ratio = (spec.lat_min + 1 - lat) * arc
        lon_ratio = (lon - spec.lon_min) * arc
        row = max(0, min(spec.size - 1, int(round(lat_ratio))))
        col = max(0, min(spec.size - 1, int(round(lon_ratio))))
        idx = (row * spec.size + col) * 2

        try:
            with spec.path.open('rb') as f:
                f.seek(idx)
                val = self._read_i16_be(f)
        except OSError:
            return None

        if val <= -32768:
            return None
        return float(val)

    def _sample_asc(self, spec: _ASCSpec, lat: float, lon: float) -> float | None:
        grid = self._asc_cache.get(spec.path)
        if grid is None:
            try:
                with spec.path.open('r', encoding='utf-8', errors='ignore') as f:
                    lines = f.readlines()[6:]
                grid = [[float(v) for v in line.strip().split()] for line in lines if line.strip()]
            except OSError:
                return None
            self._asc_cache[spec.path] = grid

        if not grid:
            return None

        col = int((lon - spec.lon_min) / spec.cellsize)
        row = int((spec.lat_max - lat) / spec.cellsize)
        row = max(0, min(len(grid) - 1, row))
        col = max(0, min(len(grid[row]) - 1, col))
        val = float(grid[row][col])
        if val == spec.nodata:
            return None
        return val

    @staticmethod
    def _read_i16_be(stream: BinaryIO) -> int:
        chunk = stream.read(2)
        if len(chunk) != 2:
            return -32768
        return int.from_bytes(chunk, byteorder='big', signed=True)


dem_loader = DEMLoader()
