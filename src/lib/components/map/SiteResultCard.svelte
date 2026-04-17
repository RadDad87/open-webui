<script lang="ts">
	export let site: { coordinates: { lat: number; lon: number }; score: number; explanation: string[] };
	export let rank = 1;
	export let intent: 'camp' | 'farm' | 'shelter' = 'camp';
	export let isTop = false;

	const iconByIntent = {
		camp: '⛺',
		farm: '🌾',
		shelter: '🛖'
	};
</script>

<div
	class={`rounded-2xl p-4 space-y-2 shadow-sm ${
		isTop
			? 'border-2 border-blue-400 bg-blue-50/60 dark:bg-blue-900/20'
			: 'border border-gray-200 dark:border-gray-800 bg-white/95 dark:bg-gray-900/95'
	}`}
>
	<div class="flex items-center justify-between">
		<h3 class="text-sm font-semibold flex items-center gap-2">
			<span>{iconByIntent[intent]}</span>
			<span>{isTop ? 'Top Candidate' : `Candidate #${rank}`}</span>
		</h3>
		<span class="text-xs font-semibold rounded-full bg-black text-white dark:bg-white dark:text-black px-2 py-1">Score: {site.score}</span>
	</div>

	<div class="text-xs text-gray-600 dark:text-gray-400">
		Lat {site.coordinates.lat}, Lon {site.coordinates.lon}
	</div>

	<div>
		<div class="text-xs font-medium mb-1">Explanation</div>
		<ul class="list-disc pl-5 text-xs space-y-1 text-gray-700 dark:text-gray-300">
			{#each site.explanation as note}
				<li>{note}</li>
			{/each}
		</ul>
	</div>
</div>
