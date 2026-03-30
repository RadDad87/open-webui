export type Z3R4HModeModelMap = Record<string, string>;

export type RoutedModelResult = {
	modelId: string | null;
	reason: 'mode-mapped' | 'selected' | 'first-available' | 'none';
	missingMappedModel: boolean;
};

export const resolveZ3R4HRoutedModelId = ({
	mode,
	modeModelMap,
	availableModelIds,
	selectedModelIds,
	atSelectedModelId
}: {
	mode: string;
	modeModelMap: Z3R4HModeModelMap;
	availableModelIds: string[];
	selectedModelIds: string[];
	atSelectedModelId?: string;
}): RoutedModelResult => {
	const normalizedAvailableIds = [...new Set((availableModelIds ?? []).filter((id) => typeof id === 'string' && id.trim().length > 0))];
	const normalizedSelectedIds = [...new Set((selectedModelIds ?? []).filter((id) => typeof id === 'string' && id.trim().length > 0))];

	const mappedId = modeModelMap?.[mode] ?? null;

	if (mappedId && normalizedAvailableIds.includes(mappedId)) {
		return { modelId: mappedId, reason: 'mode-mapped', missingMappedModel: false };
	}

	const selectedId = atSelectedModelId || normalizedSelectedIds.find((id) => !!id) || null;
	if (selectedId && normalizedAvailableIds.includes(selectedId)) {
		return {
			modelId: selectedId,
			reason: 'selected',
			missingMappedModel: Boolean(mappedId && mappedId !== selectedId)
		};
	}

	if (normalizedAvailableIds.length > 0) {
		return {
			modelId: normalizedAvailableIds[0],
			reason: 'first-available',
			missingMappedModel: Boolean(mappedId)
		};
	}

	return { modelId: null, reason: 'none', missingMappedModel: Boolean(mappedId) };
};
