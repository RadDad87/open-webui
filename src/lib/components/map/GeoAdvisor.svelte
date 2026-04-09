<script lang="ts">
	import { onMount } from 'svelte';
	import {
		geoAdvice,
		geoError,
		geoLoading,
		geoMode,
		geoQuery,
		geoRoute,
		geoSites,
		submitGeoQuery,
		type TravelMode
	} from '$lib/stores/geo';
	import RouteResultCard from '$lib/components/map/RouteResultCard.svelte';
	import SiteResultCard from '$lib/components/map/SiteResultCard.svelte';

	const modes: TravelMode[] = ['walk', 'bicycle', 'car'];

	let canvasEl: HTMLCanvasElement;
	let mapContainer: HTMLDivElement;
	let panX = 0;
	let panY = 0;
	let zoom = 1;
	let dragging = false;
	let lastX = 0;
	let lastY = 0;

	const intentIcon = (intent?: string) => {
		switch (intent) {
			case 'route':
				return '🧭';
			case 'camp':
				return '⛺';
			case 'farm':
				return '🌾';
			case 'shelter':
				return '🛖';
			default:
				return '📡';
		}
	};

	const decodePolyline6 = (encoded: string): Array<{ lat: number; lon: number }> => {
		const points: Array<{ lat: number; lon: number }> = [];
		let index = 0;
		let lat = 0;
		let lon = 0;

		while (index < encoded.length) {
			let shift = 0;
			let result = 0;
			let byte = 0;
			do {
				byte = encoded.charCodeAt(index++) - 63;
				result |= (byte & 0x1f) << shift;
				shift += 5;
			} while (byte >= 0x20);
			lat += result & 1 ? ~(result >> 1) : result >> 1;

			shift = 0;
			result = 0;
			do {
				byte = encoded.charCodeAt(index++) - 63;
				result |= (byte & 0x1f) << shift;
				shift += 5;
			} while (byte >= 0x20);
			lon += result & 1 ? ~(result >> 1) : result >> 1;

			points.push({ lat: lat / 1_000_000, lon: lon / 1_000_000 });
		}

		return points;
	};

	const runQuery = async () => {
		const q = $geoQuery.trim();
		if (!q) return;
		await submitGeoQuery(q, $geoMode);
	};

	$: routePoints = (() => {
		const trip = ($geoRoute?.data?.trip as any) ?? {};
		const legs = Array.isArray(trip.legs) ? trip.legs : [];
		const coords: Array<{ lat: number; lon: number }> = [];
		for (const leg of legs) {
			if (typeof leg?.shape === 'string' && leg.shape.length > 0) {
				coords.push(...decodePolyline6(leg.shape));
			}
		}
		return coords;
	})();

	$: sitePoints = $geoSites?.results?.map((r) => ({ lat: r.coordinates.lat, lon: r.coordinates.lon })) ?? [];
	$: allPoints = [...routePoints, ...sitePoints];

	function drawMap() {
		if (!canvasEl) return;
		const ctx = canvasEl.getContext('2d');
		if (!ctx) return;

		const width = canvasEl.width;
		const height = canvasEl.height;
		ctx.clearRect(0, 0, width, height);

		ctx.fillStyle = '#0f172a';
		ctx.fillRect(0, 0, width, height);

		ctx.strokeStyle = 'rgba(148,163,184,0.2)';
		ctx.lineWidth = 1;
		for (let x = 0; x <= width; x += 32) {
			ctx.beginPath();
			ctx.moveTo(x, 0);
			ctx.lineTo(x, height);
			ctx.stroke();
		}
		for (let y = 0; y <= height; y += 32) {
			ctx.beginPath();
			ctx.moveTo(0, y);
			ctx.lineTo(width, y);
			ctx.stroke();
		}

		if (!allPoints.length) {
			ctx.fillStyle = '#94a3b8';
			ctx.font = '14px sans-serif';
			ctx.fillText('Map ready — run a geo query to plot routes and sites.', 24, 36);
			return;
		}

		const lats = allPoints.map((p) => p.lat);
		const lons = allPoints.map((p) => p.lon);
		const minLat = Math.min(...lats);
		const maxLat = Math.max(...lats);
		const minLon = Math.min(...lons);
		const maxLon = Math.max(...lons);

		const nx = (lon: number) => {
			if (maxLon === minLon) return width / 2;
			return (((lon - minLon) / (maxLon - minLon)) * (width - 80) + 40) * zoom + panX;
		};
		const ny = (lat: number) => {
			if (maxLat === minLat) return height / 2;
			return (((maxLat - lat) / (maxLat - minLat)) * (height - 80) + 40) * zoom + panY;
		};

		if (routePoints.length > 1) {
			ctx.strokeStyle = '#38bdf8';
			ctx.lineWidth = 2;
			ctx.beginPath();
			ctx.moveTo(nx(routePoints[0].lon), ny(routePoints[0].lat));
			for (let i = 1; i < routePoints.length; i += 1) {
				ctx.lineTo(nx(routePoints[i].lon), ny(routePoints[i].lat));
			}
			ctx.stroke();
		}

		for (let i = 0; i < sitePoints.length; i += 1) {
			const p = sitePoints[i];
			ctx.beginPath();
			ctx.arc(nx(p.lon), ny(p.lat), i === 0 ? 6 : 4, 0, Math.PI * 2);
			ctx.fillStyle = i === 0 ? '#22c55e' : '#f97316';
			ctx.fill();
			ctx.strokeStyle = '#fff';
			ctx.lineWidth = 1.5;
			ctx.stroke();
		}
	}

	onMount(() => {
		const resize = () => {
			if (!mapContainer || !canvasEl) return;
			canvasEl.width = Math.max(640, mapContainer.clientWidth);
			canvasEl.height = Math.max(420, mapContainer.clientHeight);
			drawMap();
		};

		resize();
		window.addEventListener('resize', resize);
		return () => window.removeEventListener('resize', resize);
	});

	$: drawMap();
</script>

<div class="w-full h-[calc(100vh-1rem)] max-h-[100dvh] p-3 md:p-4">
	<div class="w-full h-full grid grid-cols-1 xl:grid-cols-[320px_1fr_360px] gap-3">
		<aside class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white/95 dark:bg-gray-900/95 backdrop-blur p-4 flex flex-col gap-3">
			<div>
				<div class="text-xs uppercase tracking-wide text-gray-500">Geo Advisor</div>
				<h2 class="text-lg font-semibold">Terrain Intelligence</h2>
			</div>

			<textarea
				class="w-full min-h-[120px] rounded-xl border border-gray-300 dark:border-gray-700 bg-transparent px-3 py-2 text-sm"
				placeholder="Ask for a route, camp, farm, or shelter analysis..."
				bind:value={$geoQuery}
			/>

			<div>
				<div class="text-xs font-medium mb-1">Travel Mode</div>
				<div class="grid grid-cols-3 gap-2">
					{#each modes as mode}
						<button
							class={`rounded-lg px-2 py-2 text-xs font-medium border ${
								$geoMode === mode
									? 'border-blue-500 bg-blue-500 text-white'
									: 'border-gray-300 dark:border-gray-700'
							}`}
							on:click={() => geoMode.set(mode)}
						>
							{mode}
						</button>
					{/each}
				</div>
			</div>

			<button
				class="rounded-xl bg-black text-white dark:bg-white dark:text-black px-4 py-2.5 text-sm disabled:opacity-60"
				on:click={runQuery}
				disabled={$geoLoading}
			>
				{$geoLoading ? 'Analyzing...' : 'Analyze Terrain'}
			</button>

			{#if $geoError}
				<div class="rounded-xl border border-red-300 bg-red-50 dark:bg-red-900/20 text-red-700 dark:text-red-300 p-3 text-xs">
					{$geoError}
				</div>
			{/if}
		</aside>

		<section bind:this={mapContainer} class="relative rounded-2xl border border-gray-200 dark:border-gray-800 overflow-hidden bg-slate-950 min-h-[420px]">
			<canvas
				bind:this={canvasEl}
				class="w-full h-full cursor-grab active:cursor-grabbing"
				on:mousedown={(e) => {
					dragging = true;
					lastX = e.clientX;
					lastY = e.clientY;
				}}
				on:mouseup={() => (dragging = false)}
				on:mouseleave={() => (dragging = false)}
				on:mousemove={(e) => {
					if (!dragging) return;
					panX += e.clientX - lastX;
					panY += e.clientY - lastY;
					lastX = e.clientX;
					lastY = e.clientY;
					drawMap();
				}}
				on:wheel={(e) => {
					e.preventDefault();
					zoom = Math.max(0.6, Math.min(2.2, zoom + (e.deltaY < 0 ? 0.08 : -0.08)));
					drawMap();
				}}
			/>
			<div class="absolute top-3 left-3 rounded-lg bg-black/50 text-white text-xs px-3 py-1.5">
				Interactive terrain map (drag to pan, wheel to zoom)
			</div>
			{#if $geoLoading}
				<div class="absolute inset-0 bg-black/45 flex flex-col items-center justify-center gap-3 text-white">
					<div class="w-8 h-8 border-2 border-white/40 border-t-white rounded-full animate-spin"></div>
					<div class="text-sm font-medium">Analyzing terrain...</div>
				</div>
			{/if}
		</section>

		<aside class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white/95 dark:bg-gray-900/95 backdrop-blur p-4 overflow-y-auto min-h-[420px]">
			<div class="flex items-center gap-2 mb-3">
				<span class="text-lg">{intentIcon($geoAdvice?.intent)}</span>
				<h3 class="font-semibold">Results</h3>
			</div>

			{#if !$geoLoading && !$geoAdvice && !$geoError}
				<div class="text-xs text-gray-500 rounded-xl border border-dashed border-gray-300 dark:border-gray-700 p-4">
					No results yet. Enter a query and run terrain analysis.
				</div>
			{/if}

			{#if $geoAdvice}
				<div class="mb-3 rounded-xl border border-gray-200 dark:border-gray-800 p-3 text-xs space-y-1">
					<div><span class="font-semibold">Intent:</span> {$geoAdvice.intent}</div>
					<div><span class="font-semibold">Explanation:</span> {$geoAdvice.explanation}</div>
				</div>
			{/if}

			{#if $geoRoute}
				<RouteResultCard routeData={$geoRoute.data} explanation={$geoAdvice?.explanation ?? ''} intent="route" />
			{/if}

			{#if $geoSites?.results?.length}
				<div class="space-y-3">
					{#each $geoSites.results as site, idx}
						<SiteResultCard
							{site}
							rank={idx + 1}
							intent={$geoSites.query_type}
							isTop={idx === 0}
						/>
					{/each}
				</div>
			{/if}

			{#if $geoAdvice?.warnings?.length}
				<div class="mt-3 rounded-xl border border-amber-300 bg-amber-50 dark:bg-amber-900/20 p-3 text-xs text-amber-800 dark:text-amber-200">
					<div class="font-semibold mb-1">Warnings</div>
					<ul class="list-disc pl-5 space-y-1">
						{#each $geoAdvice.warnings as warning}
							<li>{warning}</li>
						{/each}
					</ul>
				</div>
			{/if}
		</aside>
	</div>
</div>
