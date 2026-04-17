from __future__ import annotations

import os
from typing import Any

import httpx


class ValhallaClient:
    def __init__(self, base_url: str | None = None, timeout: float | None = None):
        self.base_url = (base_url or os.environ.get('VALHALLA_BASE_URL', 'http://127.0.0.1:8002')).rstrip('/')
        self.timeout = timeout or float(os.environ.get('VALHALLA_TIMEOUT_SECONDS', '10'))

    async def route(self, payload: dict[str, Any]) -> dict[str, Any]:
        return await self._post('/route', payload)

    async def map_match(self, payload: dict[str, Any]) -> dict[str, Any]:
        return await self._post('/trace_route', payload)

    async def isochrone(self, payload: dict[str, Any]) -> dict[str, Any]:
        return await self._post('/isochrone', payload)

    async def health_check(self) -> tuple[bool, int | None, str | None]:
        url = f'{self.base_url}/status'
        try:
            async with httpx.AsyncClient(timeout=self.timeout) as client:
                response = await client.get(url)
            if response.status_code < 400:
                return True, response.status_code, None
            return False, response.status_code, response.text[:500]
        except Exception as exc:
            return False, None, str(exc)

    async def _post(self, path: str, payload: dict[str, Any]) -> dict[str, Any]:
        url = f'{self.base_url}{path}'
        async with httpx.AsyncClient(timeout=self.timeout) as client:
            response = await client.post(url, json=payload)
        response.raise_for_status()
        return response.json()
