# Open WebUI 👋

![GitHub stars](https://img.shields.io/github/stars/open-webui/open-webui?style=social)
![GitHub forks](https://img.shields.io/github/forks/open-webui/open-webui?style=social)
![GitHub watchers](https://img.shields.io/github/watchers/open-webui/open-webui?style=social)
![GitHub repo size](https://img.shields.io/github/repo-size/open-webui/open-webui)
![GitHub language count](https://img.shields.io/github/languages/count/open-webui/open-webui)
![GitHub top language](https://img.shields.io/github/languages/top/open-webui/open-webui)
![GitHub last commit](https://img.shields.io/github/last-commit/open-webui/open-webui?color=red)
[![Discord](https://img.shields.io/badge/Discord-Open_WebUI-blue?logo=discord&logoColor=white)](https://discord.gg/5rJgQTnV4s)
[![](https://img.shields.io/static/v1?label=Sponsor&message=%E2%9D%A4&logo=GitHub&color=%23fe8e86)](https://github.com/sponsors/tjbck)

![Open WebUI Banner](./banner.png)

**Open WebUI is an [extensible](https://docs.openwebui.com/features/extensibility/plugin), feature-rich, and user-friendly self-hosted AI platform designed to operate entirely offline.** It supports various LLM runners like **Ollama** and **OpenAI-compatible APIs**, with **built-in inference engine** for RAG, making it a **powerful AI deployment solution**.

Passionate about open-source AI? [Join our team →](https://careers.openwebui.com/)

![Open WebUI Demo](./demo.png)

> [!TIP]  
> **Looking for an [Enterprise Plan](https://docs.openwebui.com/enterprise)?** – **[Speak with Our Sales Team Today!](https://docs.openwebui.com/enterprise)**
>
> Get **enhanced capabilities**, including **custom theming and branding**, **Service Level Agreement (SLA) support**, **Long-Term Support (LTS) versions**, and **more!**

For more information, be sure to check out our [Open WebUI Documentation](https://docs.openwebui.com/).

## Key Features of Open WebUI ⭐

- 🚀 **Effortless Setup**: Install seamlessly using Docker or Kubernetes (kubectl, kustomize or helm) for a hassle-free experience with support for both `:ollama` and `:cuda` tagged images.

- 🤝 **Ollama/OpenAI API Integration**: Effortlessly integrate OpenAI-compatible APIs for versatile conversations alongside Ollama models. Customize the OpenAI API URL to link with **LMStudio, GroqCloud, Mistral, OpenRouter, and more**.

- 🛡️ **Granular Permissions and User Groups**: By allowing administrators to create detailed user roles and permissions, we ensure a secure user environment. This granularity not only enhances security but also allows for customized user experiences, fostering a sense of ownership and responsibility amongst users.

- 📱 **Responsive Design**: Enjoy a seamless experience across Desktop PC, Laptop, and Mobile devices.

- 📱 **Progressive Web App (PWA) for Mobile**: Enjoy a native app-like experience on your mobile device with our PWA, providing offline access on localhost and a seamless user interface.

- ✒️🔢 **Full Markdown and LaTeX Support**: Elevate your LLM experience with comprehensive Markdown and LaTeX capabilities for enriched interaction.

- 🎤📹 **Hands-Free Voice/Video Call**: Experience seamless communication with integrated hands-free voice and video call features using multiple Speech-to-Text providers (Local Whisper, OpenAI, Deepgram, Azure) and Text-to-Speech engines (Azure, ElevenLabs, OpenAI, Transformers, WebAPI), allowing for dynamic and interactive chat environments.

- 🛠️ **Model Builder**: Easily create Ollama models via the Web UI. Create and add custom characters/agents, customize chat elements, and import models effortlessly through [Open WebUI Community](https://openwebui.com/) integration.

- 🐍 **Native Python Function Calling Tool**: Enhance your LLMs with built-in code editor support in the tools workspace. Bring Your Own Function (BYOF) by simply adding your pure Python functions, enabling seamless integration with LLMs.

- 💾 **Persistent Artifact Storage**: Built-in key-value storage API for artifacts, enabling features like journals, trackers, leaderboards, and collaborative tools with both personal and shared data scopes across sessions.

- 📚 **Local RAG Integration**: Dive into the future of chat interactions with groundbreaking Retrieval Augmented Generation (RAG) support using your choice of 9 vector databases and multiple content extraction engines (Tika, Docling, Document Intelligence, Mistral OCR, External loaders). Load documents directly into chat or add files to your document library, effortlessly accessing them using the `#` command before a query.

- 🔍 **Web Search for RAG**: Perform web searches using 15+ providers including `SearXNG`, `Google PSE`, `Brave Search`, `Kagi`, `Mojeek`, `Tavily`, `Perplexity`, `serpstack`, `serper`, `Serply`, `DuckDuckGo`, `SearchApi`, `SerpApi`, `Bing`, `Jina`, `Exa`, `Sougou`, `Azure AI Search`, and `Ollama Cloud`, injecting results directly into your chat experience.

- 🌐 **Web Browsing Capability**: Seamlessly integrate websites into your chat experience using the `#` command followed by a URL. This feature allows you to incorporate web content directly into your conversations, enhancing the richness and depth of your interactions.

- 🎨 **Image Generation & Editing Integration**: Create and edit images using multiple engines including OpenAI's DALL-E, Gemini, ComfyUI (local), and AUTOMATIC1111 (local), with support for both generation and prompt-based editing workflows.

- ⚙️ **Many Models Conversations**: Effortlessly engage with various models simultaneously, harnessing their unique strengths for optimal responses. Enhance your experience by leveraging a diverse set of models in parallel.

- 🔐 **Role-Based Access Control (RBAC)**: Ensure secure access with restricted permissions; only authorized individuals can access your Ollama, and exclusive model creation/pulling rights are reserved for administrators.

- 🗄️ **Flexible Database & Storage Options**: Choose from SQLite (with optional encryption), PostgreSQL, or configure cloud storage backends (S3, Google Cloud Storage, Azure Blob Storage) for scalable deployments.

- 🔍 **Advanced Vector Database Support**: Select from 9 vector database options including ChromaDB, PGVector, Qdrant, Milvus, Elasticsearch, OpenSearch, Pinecone, S3Vector, and Oracle 23ai for optimal RAG performance.

- 🔐 **Enterprise Authentication**: Full support for LDAP/Active Directory integration, SCIM 2.0 automated provisioning, and SSO via trusted headers alongside OAuth providers. Enterprise-grade user and group provisioning through SCIM 2.0 protocol, enabling seamless integration with identity providers like Okta, Azure AD, and Google Workspace for automated user lifecycle management.

- ☁️ **Cloud-Native Integration**: Native support for Google Drive and OneDrive/SharePoint file picking, enabling seamless document import from enterprise cloud storage.

- 📊 **Production Observability**: Built-in OpenTelemetry support for traces, metrics, and logs, enabling comprehensive monitoring with your existing observability stack.

- ⚖️ **Horizontal Scalability**: Redis-backed session management and WebSocket support for multi-worker and multi-node deployments behind load balancers.

- 🌐🌍 **Multilingual Support**: Experience Open WebUI in your preferred language with our internationalization (i18n) support. Join us in expanding our supported languages! We're actively seeking contributors!

- 🧩 **Pipelines, Open WebUI Plugin Support**: Seamlessly integrate custom logic and Python libraries into Open WebUI using [Pipelines Plugin Framework](https://github.com/open-webui/pipelines). Launch your Pipelines instance, set the OpenAI URL to the Pipelines URL, and explore endless possibilities. [Examples](https://github.com/open-webui/pipelines/tree/main/examples) include **Function Calling**, User **Rate Limiting** to control access, **Usage Monitoring** with tools like Langfuse, **Live Translation with LibreTranslate** for multilingual support, **Toxic Message Filtering** and much more.

- 🌟 **Continuous Updates**: We are committed to improving Open WebUI with regular updates, fixes, and new features.

Want to learn more about Open WebUI's features? Check out our [Open WebUI documentation](https://docs.openwebui.com/features) for a comprehensive overview!

---

We are incredibly grateful for the generous support of our sponsors. Their contributions help us to maintain and improve our project, ensuring we can continue to deliver quality work to our community. Thank you!

## How to Install 🚀

### Installation via Python pip 🐍

Open WebUI can be installed using pip, the Python package installer. Before proceeding, ensure you're using **Python 3.11** to avoid compatibility issues.

1. **Install Open WebUI**:
   Open your terminal and run the following command to install Open WebUI:

   ```bash
   pip install open-webui
   ```

2. **Running Open WebUI**:
   After installation, you can start Open WebUI by executing:

   ```bash
   open-webui serve
   ```

This will start the Open WebUI server, which you can access at [http://localhost:8080](http://localhost:8080)

### Quick Start with Docker 🐳

> [!NOTE]  
> Please note that for certain Docker environments, additional configurations might be needed. If you encounter any connection issues, our detailed guide on [Open WebUI Documentation](https://docs.openwebui.com/) is ready to assist you.

> [!WARNING]
> When using Docker to install Open WebUI, make sure to include the `-v open-webui:/app/backend/data` in your Docker command. This step is crucial as it ensures your database is properly mounted and prevents any loss of data.

> [!TIP]  
> If you wish to utilize Open WebUI with Ollama included or CUDA acceleration, we recommend utilizing our official images tagged with either `:cuda` or `:ollama`. To enable CUDA, you must install the [Nvidia CUDA container toolkit](https://docs.nvidia.com/dgx/nvidia-container-runtime-upgrade/) on your Linux/WSL system.

### Installation with Default Configuration

- **If Ollama is on your computer**, use this command:

  ```bash
  docker run -d -p 3000:8080 --add-host=host.docker.internal:host-gateway -v open-webui:/app/backend/data --name open-webui --restart always ghcr.io/open-webui/open-webui:main
  ```

- **If Ollama is on a Different Server**, use this command:

  To connect to Ollama on another server, change the `OLLAMA_BASE_URL` to the server's URL:

  ```bash
  docker run -d -p 3000:8080 -e OLLAMA_BASE_URL=https://example.com -v open-webui:/app/backend/data --name open-webui --restart always ghcr.io/open-webui/open-webui:main
  ```

- **To run Open WebUI with Nvidia GPU support**, use this command:

  ```bash
  docker run -d -p 3000:8080 --gpus all --add-host=host.docker.internal:host-gateway -v open-webui:/app/backend/data --name open-webui --restart always ghcr.io/open-webui/open-webui:cuda
  ```

### Installation for OpenAI API Usage Only

- **If you're only using OpenAI API**, use this command:

  ```bash
  docker run -d -p 3000:8080 -e OPENAI_API_KEY=your_secret_key -v open-webui:/app/backend/data --name open-webui --restart always ghcr.io/open-webui/open-webui:main
  ```

### Installing Open WebUI with Bundled Ollama Support

This installation method uses a single container image that bundles Open WebUI with Ollama, allowing for a streamlined setup via a single command. Choose the appropriate command based on your hardware setup:

- **With GPU Support**:
  Utilize GPU resources by running the following command:

  ```bash
  docker run -d -p 3000:8080 --gpus=all -v ollama:/root/.ollama -v open-webui:/app/backend/data --name open-webui --restart always ghcr.io/open-webui/open-webui:ollama
  ```

- **For CPU Only**:
  If you're not using a GPU, use this command instead:

  ```bash
  docker run -d -p 3000:8080 -v ollama:/root/.ollama -v open-webui:/app/backend/data --name open-webui --restart always ghcr.io/open-webui/open-webui:ollama
  ```

Both commands facilitate a built-in, hassle-free installation of both Open WebUI and Ollama, ensuring that you can get everything up and running swiftly.

After installation, you can access Open WebUI at [http://localhost:3000](http://localhost:3000). Enjoy! 😄

### Other Installation Methods

We offer various installation alternatives, including non-Docker native installation methods, Docker Compose, Kustomize, and Helm. Visit our [Open WebUI Documentation](https://docs.openwebui.com/getting-started/) or join our [Discord community](https://discord.gg/5rJgQTnV4s) for comprehensive guidance.

Look at the [Local Development Guide](https://docs.openwebui.com/getting-started/development) for instructions on setting up a local development environment.

### Troubleshooting

Encountering connection issues? Our [Open WebUI Documentation](https://docs.openwebui.com/troubleshooting/) has got you covered. For further assistance and to join our vibrant community, visit the [Open WebUI Discord](https://discord.gg/5rJgQTnV4s).

#### Open WebUI: Server Connection Error

If you're experiencing connection issues, it’s often due to the WebUI docker container not being able to reach the Ollama server at 127.0.0.1:11434 (host.docker.internal:11434) inside the container . Use the `--network=host` flag in your docker command to resolve this. Note that the port changes from 3000 to 8080, resulting in the link: `http://localhost:8080`.

**Example Docker Command**:

```bash
docker run -d --network=host -v open-webui:/app/backend/data -e OLLAMA_BASE_URL=http://127.0.0.1:11434 --name open-webui --restart always ghcr.io/open-webui/open-webui:main
```

### Keeping Your Docker Installation Up-to-Date

Check our Updating Guide available in our [Open WebUI Documentation](https://docs.openwebui.com/getting-started/updating).

### Using the Dev Branch 🌙

> [!WARNING]
> The `:dev` branch contains the latest unstable features and changes. Use it at your own risk as it may have bugs or incomplete features.

If you want to try out the latest bleeding-edge features and are okay with occasional instability, you can use the `:dev` tag like this:

```bash
docker run -d -p 3000:8080 -v open-webui:/app/backend/data --name open-webui --add-host=host.docker.internal:host-gateway --restart always ghcr.io/open-webui/open-webui:dev
```

### Offline Mode

If you are running Open WebUI in an offline environment, you can set the `HF_HUB_OFFLINE` environment variable to `1` to prevent attempts to download models from the internet.

```bash
export HF_HUB_OFFLINE=1
```

## What's Next? 🌟

Discover upcoming features on our roadmap in the [Open WebUI Documentation](https://docs.openwebui.com/roadmap/).

## License 📜

This project contains code under multiple licenses. The current codebase includes components licensed under the Open WebUI License with an additional requirement to preserve the "Open WebUI" branding, as well as prior contributions under their respective original licenses. For a detailed record of license changes and the applicable terms for each section of the code, please refer to [LICENSE_HISTORY](./LICENSE_HISTORY). For complete and updated licensing details, please see the [LICENSE](./LICENSE) and [LICENSE_HISTORY](./LICENSE_HISTORY) files.

## Support 💬

If you have any questions, suggestions, or need assistance, please open an issue or join our
[Open WebUI Discord community](https://discord.gg/5rJgQTnV4s) to connect with us! 🤝

## Star History

<a href="https://star-history.com/#open-webui/open-webui&Date">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="https://api.star-history.com/svg?repos=open-webui/open-webui&type=Date&theme=dark" />
    <source media="(prefers-color-scheme: light)" srcset="https://api.star-history.com/svg?repos=open-webui/open-webui&type=Date" />
    <img alt="Star History Chart" src="https://api.star-history.com/svg?repos=open-webui/open-webui&type=Date" />
  </picture>
</a>

---

Created by [Timothy Jaeryang Baek](https://github.com/tjbck) - Let's make Open WebUI even more amazing together! 💪

## Z3R4H Windows Local Launcher (offline/localhost)

For Z3R4H local development on Windows (no Docker), use `launch_z3r4h.bat` at the repo root.

- Starts local `llama.cpp` server
- Starts Open WebUI with local OpenAI-compatible backend settings
- Waits for both services to become reachable
- Opens Open WebUI in the default browser
- Logs to `z3r4h_launcher.log`

The launcher enforces localhost-only endpoints and uses verified Open WebUI environment variables from this codebase:
- `ENABLE_OPENAI_API=True`
- `OPENAI_API_BASE_URLS=http://127.0.0.1:<LLAMA_PORT>/v1`
- `OPENAI_API_KEYS=` (empty)

For Z3R4H offline/local startup, the launcher also applies:
- `OFFLINE_MODE=True`
- `WEBUI_AUTH=False`
- `ENABLE_LOGIN_FORM=False`
- `ENABLE_SIGNUP=False`
- `ENABLE_COMMUNITY_SHARING=False`
- `ENABLE_WEB_SEARCH=False`

This reduces cloud-first/community surfaces in the Z3R4H launcher path while preserving local chat usage.

Edit top variables in `launch_z3r4h.bat` before first run:
- `LLAMA_SERVER_EXE`
- `MODEL_PATH`
- `LLAMA_HOST`
- `LLAMA_PORT`
- `OPEN_WEBUI_START_CMD`
- `OPEN_WEBUI_URL`
- `LOG_FILE`

### Portable Package Layout (Windows)

```text
Z3R4H/
├─ launch_z3r4h.bat
├─ models/
│  └─ model.gguf
├─ runtime/
│  └─ llama.cpp/
│     └─ llama-server.exe
└─ z3r4h_launcher.log   (created at runtime)
```

Launcher defaults are now relative to `%~dp0` (the launcher directory), so the package can be moved without editing absolute paths.

Open WebUI startup options:
- **External CLI mode (default):** `open-webui serve` (requires `open-webui` in PATH)
- **Bundled runtime mode (optional):** point `OPEN_WEBUI_START_CMD` to a bundled executable path under `runtime/`

Required at runtime:
- `launch_z3r4h.bat`
- `runtime/llama.cpp/llama-server.exe` (or equivalent configured path)
- `models/<your-model>.gguf`
- Local port availability for llama.cpp + Open WebUI
- Write access for `z3r4h_launcher.log`

### Z3R4H Windows-Local Verification Checklist

1. Set launcher variables in `launch_z3r4h.bat` (`LLAMA_SERVER_EXE`, `MODEL_PATH`, ports/URL).
2. Run `launch_z3r4h.bat`.
3. Confirm launcher log shows:
   - llama.cpp started and ready
   - Open WebUI started and ready
4. Confirm browser opens the configured local URL.
5. Confirm Z3R4H Mode selector is visible in chat.
6. Send one local prompt and verify a response is returned.
7. Confirm offline-hardening behavior in launcher path:
   - no login prompt
   - community sharing disabled
   - web search disabled

### Troubleshooting (Windows Local)

- **Missing llama.cpp executable/model:** verify `LLAMA_SERVER_EXE` and `MODEL_PATH` (default package expectation: `runtime/llama.cpp/llama-server.exe` and `models/model.gguf`).
- **Open WebUI startup mode issue:**
  - `OPEN_WEBUI_START_MODE=external` requires `open-webui` in PATH
  - `OPEN_WEBUI_START_MODE=bundled` requires valid `OPEN_WEBUI_BUNDLED_EXE`
- **Port conflict before startup:** launcher now fails fast if `LLAMA_PORT` or WebUI port is already listening.
- **Readiness probe unavailable/blocked:** verify PowerShell policy or curl availability on Windows.
- **Startup timeout:** check `z3r4h_launcher.log`, then verify firewall and localhost reachability.


### Clean-Environment Launch Review (Windows)

Use `Z3R4H_CLEAN_ENV_CHECKLIST.md` for a clean-machine launch/readiness pass focused on:
- preflight requirements
- startup validation
- offline-hardening validation
- hidden assumptions and launch blockers

### Task 11 Validation Focus (Windows Local)

For Task 11, treat this as a **validation-only** pass (no behavior changes by default):
- run the execution-ready clean-machine flow in `Z3R4H_CLEAN_ENV_CHECKLIST.md`
- rank blockers as P0/P1/P2 for launch-readiness decisions
- record hidden assumptions with concrete repro evidence

Highest-risk items to validate first:
- default external `open-webui` PATH dependency
- PowerShell readiness-probe assumptions
- local port conflicts (11434/8080)
- packaged model path/filename mismatch


### Task 12 Blocker-Fix Focus (Windows Local)

Task 12 hardens clean-machine startup by improving launcher preflight clarity and failure diagnostics only (no product/backend feature expansion):
- explicit Open WebUI startup-mode branching (external vs bundled)
- readiness-probe fallback diagnostics
- model-path contract clarity
- fail-fast local port conflict checks


### Task 13 Release-Candidate Checklist (Windows Portable Package)

Use this RC gate before any installer/wrapper work:
1. Scope and intent confirmed (documentation/readiness-only gate).
2. Package manifest complete (launcher/runtime/model artifacts present).
3. Runtime prerequisites confirmed on clean Windows machine.
4. Preflight checks pass (startup mode, paths, ports, log write access).
5. Cold launch validation passes (llama + Open WebUI ready).
6. Operator smoke checks pass (UI opens, one local response).
7. No unresolved P0/P1 blockers in checklist blocker register.
8. Evidence captured and signoff recorded.


### Task 14 Validation Execution Support (Windows Local)

For clean-machine RC execution, run validation in this exact order:
1. Session header capture (operator/date/machine/build).
2. Package manifest verification.
3. Runtime prerequisite verification.
4. Launcher preflight checks (mode/paths/ports/log write).
5. Cold launch execution and readiness confirmation.
6. Operator smoke check (UI opens + one local prompt/response).
7. Blocker classification (P0/P1/P2) and disposition.
8. RC signoff decision with evidence pack attached.

Use `Z3R4H_CLEAN_ENV_CHECKLIST.md` as the source of truth for pass/fail recording.


### Task 15 RC Run Result Capture (Windows Local)

After executing validation, record outcomes using the Task 15 run-result template in `Z3R4H_CLEAN_ENV_CHECKLIST.md`:
- per-step pass/fail/blocked status
- evidence references per step
- blocker rollup (P0/P1/P2)
- final release recommendation (`GO`, `GO-WITH-RISK`, `NO-GO`)


### Task 16 v1 Shipping Format Decision (Windows)

Compared options for v1:
1. Portable BAT-launched package (current baseline)
2. Wrapped desktop `.exe` / shell approach (intended v1 target)

**v1 Recommendation:** Ship with the wrapped desktop `.exe` / shell approach as the primary operator entry point.

Rationale for .exe-first:
- improves operator-first launch experience by reducing manual launcher handling
- provides a clearer product-like entry path for v1 distribution
- reduces perceived complexity for first-time Windows operators

BAT path role during transition:
- retain BAT launch flow as controlled fallback while exe-first gates are validated
- do not position BAT as primary v1 operator experience

Exe-first gating conditions before release decision:
- repeated RC runs show no unresolved P0 launch blockers in exe-first flow
- failure diagnostics remain operator-visible and supportable
- portability expectations are preserved for packaged runtime/model assets
- support burden is acceptable versus BAT baseline in pilot validation


### Task 17 .exe Wrapper Implementation Plan (v1 Path)

Task 17 defines the implementation plan only (no wrapper code yet):
- wrapper technology options and recommendation
- runtime startup orchestration sequence
- diagnostics/logging expectations
- BAT launcher fallback/support role

v1 intended entrypoint remains `.exe` / shell wrapper, with BAT preserved as fallback/support.


### Task 18 Concrete .exe Wrapper Implementation Plan (Build-Ready)

Task 18 adds the build-ready implementation plan for the `.exe` wrapper path (planning only, no code):
- recommended wrapper technology
- concrete wrapper project layout
- startup orchestration and readiness sequence
- logging/error-handling model
- BAT fallback/support path expectations

For v1, `.exe` remains the intended primary entrypoint; BAT remains fallback/support only.


### Task 19 Minimal Wrapper Scaffold (Implemented)

Added initial native C#/.NET wrapper scaffold under `wrapper/` for `.exe`-first v1 path:
- project structure and entrypoint
- config/path resolution scaffold
- orchestration/readiness/logging stubs
- BAT fallback hook scaffold

No full runtime orchestration, installer, or build automation was implemented in this task.


### Task 20 First Working Wrapper Milestone

The wrapper now provides a first working milestone for `.exe`-first path by delivering:
- config/path validation
- structured logging and error categories
- structured exit codes
- BAT fallback invocation path (`auto`/`always`/`never`)

Full runtime orchestration/readiness/browser launch remain intentionally deferred.


### Task 21 First Live Llama Orchestration Milestone

Wrapper now performs first live orchestration by launching llama and validating llama readiness.
This milestone still does **not** start Open WebUI or launch a browser.
BAT fallback remains the support/recovery path.


### Task 22 Open WebUI + Dual-Service Readiness Milestone

Wrapper now supports dual-service startup readiness:
1) launch/validate llama
2) launch/validate Open WebUI

This milestone still excludes browser launch and advanced supervision.


### Task 23 Browser Launch on Dual-Service Readiness

Wrapper now opens the configured local Open WebUI URL in the default browser
only after both llama and Open WebUI readiness checks pass.

Fallback behavior remains unchanged for earlier startup failures (`auto` / `always` / `never`).
Browser-launch-only failures are returned as structured wrapper outcomes.


### Task 24 Wrapper-First RC Validation Execution Support

RC validation workflow now explicitly treats the wrapper entrypoint as the
primary clean-machine validation path for `.exe`-first shipping.

Validation guidance now records:
- wrapper stage-by-stage outcomes (config, llama, webui, browser-launch)
- wrapper exit code + terminal stage
- BAT fallback evidence only when wrapper failure triggers support handoff
