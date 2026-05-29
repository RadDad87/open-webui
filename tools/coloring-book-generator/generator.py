import argparse
import os
import sys
from typing import Literal

from openai import OpenAI

try:
    from anthropic import Anthropic
except Exception:
    Anthropic = None

Provider = Literal["openai", "claude"]


def load_dotenv_if_present(dotenv_path: str = ".env") -> None:
    if not os.path.exists(dotenv_path):
        return

    with open(dotenv_path, "r", encoding="utf-8") as file:
        for raw_line in file:
            line = raw_line.strip()
            if not line or line.startswith("#") or "=" not in line:
                continue

            key, value = line.split("=", 1)
            key = key.strip()
            value = value.strip().strip('"').strip("'")
            if key and key not in os.environ:
                os.environ[key] = value


def build_user_prompt(theme: str, age_range: str, complexity: str, extra_notes: str) -> str:
    base = (
        "Create one high-quality coloring-book image prompt. "
        "The output should describe black-and-white line art only, with clean outlines, no shading, "
        "no grayscale, no color, no text, and a plain white background."
    )

    details = [
        f"Theme: {theme}",
        f"Target age range: {age_range}",
        f"Complexity level: {complexity}",
    ]

    if extra_notes.strip():
        details.append(f"Extra notes: {extra_notes.strip()}")

    formatting = (
        "Return exactly two sections:\n"
        "1) TITLE: a short page title\n"
        "2) PROMPT: the final generation prompt"
    )

    return f"{base}\n\n" + "\n".join(details) + "\n\n" + formatting


def generate_with_openai(model: str, prompt: str, api_key: str | None = None) -> str:
    api_key = api_key or os.getenv("OPENAI_API_KEY")
    if not api_key:
        raise RuntimeError("OPENAI_API_KEY is not set.")

    client = OpenAI(api_key=api_key)
    response = client.responses.create(
        model=model,
        input=[
            {
                "role": "system",
                "content": [
                    {
                        "type": "input_text",
                        "text": "You are an expert at writing safe, print-friendly coloring-book prompts.",
                    }
                ],
            },
            {"role": "user", "content": [{"type": "input_text", "text": prompt}]},
        ],
        temperature=0.8,
    )

    return response.output_text.strip()


def generate_with_claude(model: str, prompt: str, api_key: str | None = None) -> str:
    if Anthropic is None:
        raise RuntimeError("anthropic package is not installed. Install it or switch to OpenAI.")

    api_key = api_key or os.getenv("ANTHROPIC_API_KEY")
    if not api_key:
        raise RuntimeError("ANTHROPIC_API_KEY is not set.")

    client = Anthropic(api_key=api_key)
    response = client.messages.create(
        model=model,
        max_tokens=600,
        temperature=0.8,
        system="You are an expert at writing safe, print-friendly coloring-book prompts.",
        messages=[{"role": "user", "content": prompt}],
    )

    chunks = []
    for content in response.content:
        text = getattr(content, "text", "")
        if text:
            chunks.append(text)

    return "\n".join(chunks).strip()


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Coloring Book Prompt Generator")
    parser.add_argument("--theme", required=True, help="What should this page be about?")
    parser.add_argument("--age-range", default="6-10", help="Example: 4-6, 6-10, 10-14")
    parser.add_argument(
        "--complexity",
        default="medium",
        choices=["simple", "medium", "detailed"],
        help="Line/detail complexity",
    )
    parser.add_argument("--extra-notes", default="", help="Optional style constraints")

    parser.add_argument(
        "--provider",
        default="openai",
        choices=["openai", "claude"],
        help="Default is OpenAI. Claude is used only when explicitly selected.",
    )
    parser.add_argument("--openai-model", default="gpt-4.1-mini")
    parser.add_argument("--claude-model", default="claude-3-7-sonnet-latest")
    parser.add_argument(
        "--openai-api-key",
        default=None,
        help="OpenAI API key. If omitted, OPENAI_API_KEY is used.",
    )
    parser.add_argument(
        "--anthropic-api-key",
        default=None,
        help="Anthropic API key. If omitted, ANTHROPIC_API_KEY is used.",
    )

    return parser.parse_args()


def main() -> int:
    load_dotenv_if_present()
    args = parse_args()
    user_prompt = build_user_prompt(
        theme=args.theme,
        age_range=args.age_range,
        complexity=args.complexity,
        extra_notes=args.extra_notes,
    )

    provider: Provider = args.provider

    try:
        if provider == "claude":
            output = generate_with_claude(
                args.claude_model,
                user_prompt,
                api_key=args.anthropic_api_key,
            )
        else:
            output = generate_with_openai(
                args.openai_model,
                user_prompt,
                api_key=args.openai_api_key,
            )

        print(output)
        return 0
    except Exception as exc:
        print(f"Error: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
