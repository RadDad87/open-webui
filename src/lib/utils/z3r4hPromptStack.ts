import coreSystem from '$lib/prompts/z3r4h/core_system.txt?raw';
import failureBehavior from '$lib/prompts/z3r4h/failure_behavior.txt?raw';
import generalAdvisor from '$lib/prompts/z3r4h/modes/general_advisor.txt?raw';
import medicalReference from '$lib/prompts/z3r4h/modes/medical_reference.txt?raw';
import engineeringRepair from '$lib/prompts/z3r4h/modes/engineering_repair.txt?raw';
import navigationTerrain from '$lib/prompts/z3r4h/modes/navigation_terrain.txt?raw';
import lowPowerMode from '$lib/prompts/z3r4h/modes/low_power_mode.txt?raw';

export const Z3R4H_MODE_TO_PROMPT: Record<string, string> = {
	'General Advisor': generalAdvisor,
	'Medical Reference': medicalReference,
	'Engineering & Repair': engineeringRepair,
	'Navigation & Terrain': navigationTerrain,
	'Low Power Mode': lowPowerMode
};

export const composeZ3R4HPromptStack = (mode: string): string => {
	const modePrompt = Z3R4H_MODE_TO_PROMPT[mode] ?? generalAdvisor;
	return [coreSystem.trim(), modePrompt.trim(), failureBehavior.trim()].join('\n\n');
};
