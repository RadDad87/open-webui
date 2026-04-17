import { describe, expect, it } from 'vitest';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

describe('maps flow ui contracts', () => {
	const geoAdvisor = readFileSync(resolve('src/lib/components/map/GeoAdvisor.svelte'), 'utf-8');
	const routeCard = readFileSync(resolve('src/lib/components/map/RouteResultCard.svelte'), 'utf-8');
	const siteCard = readFileSync(resolve('src/lib/components/map/SiteResultCard.svelte'), 'utf-8');

	it('contains route result render hooks', () => {
		expect(geoAdvisor).toContain('RouteResultCard');
		expect(routeCard).toContain('Difficulty');
		expect(routeCard).toContain('Elevation Gain');
	});

	it('contains site result render hooks', () => {
		expect(geoAdvisor).toContain('SiteResultCard');
		expect(siteCard).toContain('Top Candidate');
		expect(siteCard).toContain('Score');
	});

	it('contains loading state message', () => {
		expect(geoAdvisor).toContain('Analyzing terrain...');
	});

	it('contains error state rendering', () => {
		expect(geoAdvisor).toContain('$geoError');
	});
});
