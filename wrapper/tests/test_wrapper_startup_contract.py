from pathlib import Path


def _read(path: str) -> str:
    return Path(path).read_text(encoding='utf-8')


def test_wrapper_startup_all_healthy_path_present():
    text = _read('wrapper/src/Z3R4H.Wrapper/Orchestration/RuntimeOrchestrator.cs')
    assert 'AI service startup' in text
    assert 'Backend startup' in text
    assert 'Routing engine startup' in text
    assert 'browser launch requested for' in text


def test_wrapper_startup_valhalla_unavailable_fallback_to_basic_mode_present():
    text = _read('wrapper/src/Z3R4H.Wrapper/Orchestration/RuntimeOrchestrator.cs')
    assert 'Routing unavailable; falling back to basic maps mode' in text
    assert 'browser-launch-basic' in text


def test_wrapper_startup_backend_unavailable_path_present():
    text = _read('wrapper/src/Z3R4H.Wrapper/Orchestration/RuntimeOrchestrator.cs')
    assert 'backend-readiness' in text
    assert 'StepFailure("Backend"' in text


def test_wrapper_startup_ai_unavailable_path_present():
    text = _read('wrapper/src/Z3R4H.Wrapper/Orchestration/RuntimeOrchestrator.cs')
    assert 'No usable local AI runtime found' in text
    assert 'ai-launch' in text
