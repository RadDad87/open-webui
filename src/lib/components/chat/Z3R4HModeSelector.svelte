<script lang="ts">
	const Z3R4H_MODES = [
		'General Advisor',
		'Medical Reference',
		'Engineering & Repair',
		'Navigation & Terrain',
		'Low Power Mode'
	];

	export let value: string = Z3R4H_MODES[0];
</script>

<div class="flex items-center gap-2 w-full md:w-auto">
	<span class="text-[11px] uppercase tracking-wide text-gray-500 dark:text-gray-400 whitespace-nowrap"
		>Z3R4H Mode</span
	>
	<select
		class="px-2 py-1 text-xs rounded-lg bg-white/80 dark:bg-gray-900/70 border border-gray-200 dark:border-gray-800 text-gray-800 dark:text-gray-100 max-w-full"
		bind:value
	>
		{#each Z3R4H_MODES as mode}
			<option value={mode}>{mode}</option>
		{/each}
	</select>
</div>
