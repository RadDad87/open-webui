<script lang="ts">
	export let routeData: Record<string, any> | null = null;
	export let explanation = '';
	export let intent: 'route' | 'camp' | 'farm' | 'shelter' = 'route';

	$: terrain = (routeData?.terrain as Record<string, any>) ?? null;
	$: difficulty = (terrain?.difficulty ?? 'unknown') as string;
	$: elevationGain = terrain?.elevation_gain_m ?? 0;
	$: risks = Array.isArray(terrain?.risks) ? terrain.risks : [];

	const difficultyClass = (value: string) => {
		switch (value) {
			case 'easy':
				return 'text-emerald-600 bg-emerald-50 dark:bg-emerald-900/30';
			case 'moderate':
				return 'text-yellow-700 bg-yellow-50 dark:bg-yellow-900/30';
			case 'hard':
				return 'text-orange-700 bg-orange-50 dark:bg-orange-900/30';
			case 'extreme':
				return 'text-red-700 bg-red-50 dark:bg-red-900/30';
			default:
				return 'text-gray-700 bg-gray-50 dark:bg-gray-800';
		}
	};

	const intentIcon = (value: string) => (value === 'route' ? '🧭' : '📍');
</script>

<div class="rounded-2xl border border-gray-200 dark:border-gray-800 p-4 bg-white/95 dark:bg-gray-900/95 backdrop-blur space-y-3 shadow-sm">
	<div class="flex items-center justify-between">
		<h3 class="text-sm font-semibold flex items-center gap-2">
			<span>{intentIcon(intent)}</span>
			<span>Route Intelligence</span>
		</h3>
		<span class={`text-xs px-2 py-1 rounded-full font-semibold ${difficultyClass(difficulty)}`}>{difficulty}</span>
	</div>

	<p class="text-xs text-gray-600 dark:text-gray-300">{explanation}</p>

	<div class="grid grid-cols-2 gap-2 text-xs">
		<div class="rounded-lg border border-gray-100 dark:border-gray-800 p-2">
			<div class="text-gray-500">Elevation Gain</div>
			<div class="font-semibold">{elevationGain} m</div>
		</div>
		<div class="rounded-lg border border-gray-100 dark:border-gray-800 p-2">
			<div class="text-gray-500">Terrain Risk Count</div>
			<div class="font-semibold">{risks.length}</div>
		</div>
	</div>

	<div>
		<div class="text-xs font-medium mb-1">Risks</div>
		{#if risks.length}
			<ul class="list-disc pl-5 text-xs space-y-1 text-gray-700 dark:text-gray-300">
				{#each risks as risk}
					<li>{risk}</li>
				{/each}
			</ul>
		{:else}
			<div class="text-xs text-gray-500">No terrain risks reported.</div>
		{/if}
	</div>
</div>
