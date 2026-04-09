import { writable } from 'svelte/store';

export type TravelMode = 'walk' | 'bicycle' | 'car';
export type GeoIntent = 'route' | 'camp' | 'farm' | 'shelter';

export type GeoCoordinate = {
	lat: number;
	lon: number;
};

type AdviceResponse = {
	ok: boolean;
	intent: GeoIntent;
	structured_query: Record<string, unknown>;
	selected_results: Array<Record<string, unknown>>;
	explanation: string;
	warnings?: string[];
	assumptions?: string[];
};

type SiteResult = {
	coordinates: GeoCoordinate;
	score: number;
	explanation: string[];
};

type SiteSearchResponse = {
	ok: boolean;
	query_type: 'camp' | 'farm' | 'shelter';
	results: SiteResult[];
};

type RouteResponse = {
	ok: boolean;
	data: Record<string, unknown>;
};

export const geoQuery = writable('');
export const geoMode = writable<TravelMode>('walk');
export const geoLoading = writable(false);
export const geoError = writable<string | null>(null);

export const geoAdvice = writable<AdviceResponse | null>(null);
export const geoRoute = writable<RouteResponse | null>(null);
export const geoSites = writable<SiteSearchResponse | null>(null);

const COSTING_BY_MODE: Record<TravelMode, string> = {
	walk: 'pedestrian',
	bicycle: 'bicycle',
	car: 'auto'
};

const DEFAULT_ORIGIN: GeoCoordinate = { lat: 47.6062, lon: -122.3321 };
const DEFAULT_DESTINATION: GeoCoordinate = { lat: 47.6205, lon: -122.3493 };

export const submitGeoQuery = async (question: string, mode: TravelMode) => {
	geoLoading.set(true);
	geoError.set(null);
	geoRoute.set(null);
	geoSites.set(null);

	try {
		const advicePayload = {
			question,
			coordinates: DEFAULT_ORIGIN,
			destination: DEFAULT_DESTINATION,
			radius_km: 8,
			costing: COSTING_BY_MODE[mode],
			constraints: {
				access_mode: mode === 'car' ? 'vehicle' : mode === 'bicycle' ? 'mixed' : 'foot'
			}
		};

		const adviceRes = await fetch('/api/geo/advice', {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify(advicePayload)
		});

		if (!adviceRes.ok) {
			throw new Error(`Geo advice request failed (${adviceRes.status})`);
		}

		const advice = (await adviceRes.json()) as AdviceResponse;
		geoAdvice.set(advice);

		if (advice.intent === 'route') {
			const routePayload = {
				locations: [DEFAULT_ORIGIN, DEFAULT_DESTINATION],
				costing: COSTING_BY_MODE[mode],
				units: 'kilometers'
			};

			const routeRes = await fetch('/api/geo/route?include_terrain=true', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(routePayload)
			});
			if (!routeRes.ok) {
				throw new Error(`Route request failed (${routeRes.status})`);
			}
			geoRoute.set((await routeRes.json()) as RouteResponse);
		} else {
			const queryType = advice.intent as 'camp' | 'farm' | 'shelter';
			const sitePayload = {
				query_type: queryType,
				coordinates: DEFAULT_ORIGIN,
				radius_km: 8,
				constraints: {
					access_mode: mode === 'car' ? 'vehicle' : mode === 'bicycle' ? 'mixed' : 'foot'
				}
			};

			const siteRes = await fetch('/api/geo/site-search', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(sitePayload)
			});
			if (!siteRes.ok) {
				throw new Error(`Site search request failed (${siteRes.status})`);
			}
			geoSites.set((await siteRes.json()) as SiteSearchResponse);
		}
	} catch (err) {
		const message = err instanceof Error ? err.message : 'Unknown geo request error';
		geoError.set(message);
	} finally {
		geoLoading.set(false);
	}
};
