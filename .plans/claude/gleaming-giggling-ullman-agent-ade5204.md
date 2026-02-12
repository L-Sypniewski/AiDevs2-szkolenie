# Research: Latest AI Models 2025-2026

**Date**: 2026-02-13
**Query**: Research the latest AI models available in 2025-2026 for: 1) OpenAI API models (GPT-4o, GPT-4.1, o1, o3, o4-mini, etc.), 2) GitHub Models available models, 3) Popular Ollama models. Focus on current production-ready models, their model IDs, and deprecation status.

## Summary

The AI landscape has significantly evolved in 2025-2026. OpenAI has released the GPT-5 series (GPT-5, GPT-5-mini, GPT-5-nano, GPT-5.1, GPT-5.2) as their flagship models, with the o-series reasoning models (o3, o4-mini) continuing for specialized tasks. Many legacy models including GPT-4o, GPT-4.5-preview, and o1-series are being deprecated throughout 2025-2026. GitHub Models provides free access to a curated selection from OpenAI, Microsoft (Phi series), AI21 Labs, and others. Ollama hosts over 100+ open-source models with DeepSeek-R1, Llama 3.x series, Qwen3, and Gemma3 being the most popular for local deployment.

---

## 1. OpenAI API Models (2025-2026)

### Current Production Models

#### GPT-5 Series (Flagship - Released 2025)
| Model ID | Description | Status |
|----------|-------------|--------|
| `gpt-5.2` | Best model for coding and agentic tasks across industries | **CURRENT** |
| `gpt-5.2-pro` | Version producing smarter and more precise responses | **CURRENT** |
| `gpt-5.2-codex` | Most intelligent coding model for long-horizon agentic tasks | **CURRENT** |
| `gpt-5.2-chat-latest` | GPT-5.2 model used in ChatGPT | **CURRENT** |
| `gpt-5.1` | Best model for coding/agentic tasks with configurable reasoning effort | **CURRENT** |
| `gpt-5.1-pro` | Smarter and more precise responses version | **CURRENT** |
| `gpt-5.1-codex` | Optimized for agentic coding in Codex | **CURRENT** |
| `gpt-5.1-codex-max` | Optimized for long-running tasks | **CURRENT** |
| `gpt-5.1-codex-mini` | Smaller, more cost-effective version | **CURRENT** |
| `gpt-5.1-chat-latest` | GPT-5.1 model used in ChatGPT | **CURRENT** |
| `gpt-5` | Previous intelligent reasoning model | **CURRENT** |
| `gpt-5-pro` | Version producing smarter and more precise responses | **CURRENT** |
| `gpt-5-codex` | Optimized for agentic coding in Codex | **CURRENT** |
| `gpt-5-mini` | Faster, cost-efficient version for well-defined tasks | **CURRENT** |
| `gpt-5-nano` | Fastest, most cost-efficient version of GPT-5 | **CURRENT** |
| `gpt-5-chat-latest` | GPT-5 model used in ChatGPT | **CURRENT** |

#### GPT-4.1 Series
| Model ID | Description | Status |
|----------|-------------|--------|
| `gpt-4.1` | Smartest non-reasoning model | **CURRENT** |
| `gpt-4.1-mini` | Smaller, faster version of GPT-4.1 | **CURRENT** |
| `gpt-4.1-nano` | Fastest, most cost-efficient version of GPT-4.1 | **CURRENT** |

#### O-Series Reasoning Models
| Model ID | Description | Status |
|----------|-------------|--------|
| `o3` | Reasoning model for complex tasks (succeeded by GPT-5) | **CURRENT (Legacy)** |
| `o3-mini` | Small model alternative to o3 | **CURRENT** |
| `o3-pro` | Version with more compute for better responses | **CURRENT** |
| `o3-deep-research` | Most powerful deep research model | **CURRENT** |
| `o4-mini` | Fast, cost-efficient reasoning model (succeeded by GPT-5-mini) | **CURRENT (Legacy)** |
| `o4-mini-deep-research` | Faster, more affordable deep research model | **CURRENT** |

#### Audio/Realtime Models
| Model ID | Description | Status |
|----------|-------------|--------|
| `gpt-audio` | Audio inputs and outputs with Chat Completions API | **CURRENT** |
| `gpt-audio-mini` | Cost-efficient version of GPT Audio | **CURRENT** |
| `gpt-realtime` | Realtime text and audio inputs/outputs | **CURRENT** |
| `gpt-realtime-mini` | Cost-efficient version of GPT Realtime | **CURRENT** |

#### Image/Video Models
| Model ID | Description | Status |
|----------|-------------|--------|
| `gpt-image-1.5` | State-of-the-art image generation model | **CURRENT** |
| `chatgpt-image-latest` | Image model used in ChatGPT | **CURRENT** |
| `sora-2` | Flagship video generation with synced audio | **CURRENT** |
| `sora-2-pro` | Most advanced synced-audio video generation | **CURRENT** |

#### Search Models
| Model ID | Description | Status |
|----------|-------------|--------|
| `gpt-4o-search-preview` | GPT model for web search in Chat Completions | **CURRENT** |
| `gpt-4o-mini-search-preview` | Fast, affordable small model for web search | **CURRENT** |

#### Open-Weight Models
| Model ID | Description | Status |
|----------|-------------|--------|
| `gpt-oss-120b` | Most powerful open-weight model, fits into H100 GPU | **CURRENT** |
| `gpt-oss-20b` | Medium-sized open-weight model for low latency | **CURRENT** |

#### Embedding Models
| Model ID | Description | Status |
|----------|-------------|--------|
| `text-embedding-3-large` | Most capable embedding model | **CURRENT** |
| `text-embedding-3-small` | Efficient embedding model | **CURRENT** |

#### Moderation
| Model ID | Description | Status |
|----------|-------------|--------|
| `omni-moderation-latest` | Identify harmful content in text and images | **CURRENT** |

### Deprecated/Deprecating Models

| Shutdown Date | Model ID | Replacement |
|---------------|----------|-------------|
| 2026-02-17 | `chatgpt-4o-latest` | `gpt-5.1-chat-latest` |
| 2026-02-12 | `codex-mini-latest` | `gpt-5-codex-mini` |
| 2026-03-24 | `gpt-4o-realtime-preview` (all variants) | `gpt-realtime` |
| 2026-03-24 | `gpt-4o-audio-preview` | `gpt-audio` |
| 2026-03-26 | `gpt-4-0314`, `gpt-4-1106-preview`, `gpt-4-turbo-preview` | `gpt-5` or `gpt-4.1*` |
| 2026-05-12 | `dall-e-2`, `dall-e-3` | `gpt-image-1` |
| 2026-09-28 | `gpt-3.5-turbo-instruct`, `babbage-002`, `davinci-002` | `gpt-5-mini` |
| 2025-07-14 | `gpt-4.5-preview` | `gpt-4.1` |
| 2025-07-28 | `o1-preview` | `o3` |
| 2025-10-27 | `o1-mini` | `o4-mini` |
| 2025-10-27 | `text-moderation-*` | `omni-moderation` |
| 2026-02-27 | Realtime API Beta (v1) | Realtime API |
| 2026-08-26 | Assistants API | Responses API |

---

## 2. GitHub Models Catalog

GitHub Models provides free access to AI models via Azure AI inference endpoint (`https://models.inference.ai.azure.com`).

### Available Models by Provider

#### OpenAI Models
| Model ID | Description |
|----------|-------------|
| `openai/gpt-4.1` | Smartest non-reasoning model (1M context, 32K output) |
| `openai/gpt-4.1-mini` | Smaller, faster version |
| `openai/gpt-4.1-nano` | Fastest, most cost-efficient version |
| `openai/gpt-4o` | Fast, intelligent, flexible GPT model |
| `openai/gpt-4o-mini` | Fast, affordable small model |
| `openai/o3-mini` | Cost-efficient reasoning model |
| `openai/o3` | Full reasoning model for complex tasks |
| `openai/o4-mini` | Fast, cost-efficient reasoning model |
| `openai/text-embedding-3-large` | Most capable embedding model |
| `openai/text-embedding-3-small` | Efficient embedding model |

#### Microsoft Phi Series
| Model ID | Description |
|----------|-------------|
| `microsoft/phi-4` | 14B parameter state-of-the-art open model |
| `microsoft/phi-4-mini` | 3.8B parameters with function calling support |
| `microsoft/phi-4-reasoning` | State-of-the-art open-weight reasoning model |
| `microsoft/phi-4-multimodal-instruct` | Small multimodal model (text, audio, image) |
| `microsoft/phi-3.5` | Lightweight 3.8B parameter model |
| `microsoft/phi-3` | 3.8B and 14B models |

#### AI21 Labs
| Model ID | Description |
|----------|-------------|
| `ai21/jamba-1.5-large` | 398B parameters (94B active), 256K context, multilingual |

### API Access Pattern
```bash
curl -L \
  -H "Accept: application/vnd.github+json" \
  -H "Authorization: Bearer <YOUR-TOKEN>" \
  -H "X-GitHub-Api-Version: 2022-11-28" \
  https://models.github.ai/catalog/models
```

---

## 3. Popular Ollama Models (2025-2026)

### Top Models by Pull Count

#### Reasoning Models
| Model | Sizes | Pulls | Description |
|-------|-------|-------|-------------|
| `deepseek-r1` | 1.5b, 7b, 8b, 14b, 32b, 70b, 671b | 78M | Open reasoning model, performance approaching O3 |
| `qwq` | 32b | 2M | Qwen reasoning model |
| `phi4-reasoning` | 14b | 1.2M | Microsoft reasoning model |

#### General Purpose Models
| Model | Sizes | Pulls | Description |
|-------|-------|-------|-------------|
| `llama3.1` | 8b, 70b, 405b | 110M | Meta's state-of-the-art model |
| `llama3.2` | 1b, 3b | 56.7M | Meta's small models |
| `llama3.3` | 70b | 3.3M | 70B model matching 405B performance |
| `llama3` | 8b, 70b | 15.1M | Original Llama 3 |
| `llama4` | 16x17b, 128x17b | 1.2M | Meta's latest multimodal models |
| `mistral` | 7b | 25.1M | Mistral AI 7B (v0.3) |
| `mixtral` | 8x7b, 8x22b | 1.8M | MoE model by Mistral AI |
| `qwen2.5` | 0.5b-72b | 21M | Alibaba's pretrained models (128K context) |
| `qwen3` | 0.6b-235b | 19M | Latest Qwen with MoE models |
| `gemma3` | 270m-27b | 31.7M | Google's most capable single-GPU model |
| `gemma2` | 2b, 9b, 27b | 15.6M | Google Gemma 2 |
| `phi3` | 3.8b, 14b | 15.9M | Microsoft's lightweight models |
| `phi4` | 14b | 7.2M | Microsoft 14B model |

#### Coding Models
| Model | Sizes | Pulls | Description |
|-------|-------|-------|-------------|
| `qwen2.5-coder` | 0.5b-32b | 11M | Code-specific Qwen models |
| `qwen3-coder` | 30b, 480b | 3M | Agentic and coding tasks |
| `codellama` | 7b-70b | 4.2M | Meta's code generation model |
| `deepseek-coder-v2` | 16b, 236b | 1.6M | MoE code model (GPT4-Turbo level) |
| `codestral` | 22b | 793.7K | Mistral's code model |
| `devstral` | 24b | 728.9K | Best open-source coding agent model |

#### Vision Models
| Model | Sizes | Pulls | Description |
|-------|-------|-------|-------------|
| `llava` | 7b, 13b, 34b | 12.8M | Multimodal vision-language model |
| `llama3.2-vision` | 11b, 90b | 3.8M | Meta's image reasoning models |
| `qwen3-vl` | 2b-235b | 1.4M | Most powerful Qwen VL model |
| `minicpm-v` | 8b | 4.5M | Vision-language MLLMs |

#### Embedding Models
| Model | Size | Pulls | Description |
|-------|------|-------|-------------|
| `nomic-embed-text` | - | 54M | High-performing embedding with large context |
| `mxbai-embed-large` | 335m | 7.3M | SOTA embedding from mixedbread.ai |
| `bge-m3` | 567m | 3.2M | Multi-functionality, multi-linguality |

#### OpenAI Open-Weight Models
| Model | Size | Pulls | Description |
|-------|------|-------|-------------|
| `gpt-oss` | 20b, 120b | 6.8M | OpenAI's open-weight reasoning models |

### Recommended Ollama Commands
```bash
# Most popular general model
ollama pull llama3.1

# Best reasoning model
ollama pull deepseek-r1

# Best coding model
ollama pull qwen2.5-coder

# Best small model
ollama pull phi4-mini

# Best embedding model
ollama pull nomic-embed-text
```

---

## Key Findings

### OpenAI
1. **GPT-5 series is the new flagship** - GPT-5.2 is now the most advanced model for coding and agentic tasks
2. **Mass deprecation wave** - GPT-4o, GPT-4.5, o1-series are all being deprecated in 2025-2026
3. **o-series continues** - o3 and o4-mini remain available for specialized reasoning tasks
4. **New model categories** - GPT-image-1.5, Sora-2, GPT-audio, GPT-realtime for multimodal
5. **Open-weight offering** - gpt-oss-120b and gpt-oss-20b for self-hosted deployments

### GitHub Models
1. **Free tier access** - Provides free access to GPT-4.1, Phi-4, and other models
2. **Azure AI endpoint** - Uses `https://models.inference.ai.azure.com` for inference
3. **Curated selection** - Focuses on production-ready models from OpenAI, Microsoft, AI21

### Ollama
1. **DeepSeek-R1 dominates** - 78M pulls, best open reasoning model rivaling O3
2. **Llama 3.1 still king** - 110M pulls, most popular general-purpose model
3. **Gemma3 rising** - Google's new model with excellent single-GPU performance
4. **Qwen3 comprehensive** - Wide range from 0.6B to 235B parameters

---

## Model Recommendations by Use Case

| Use Case | OpenAI API | GitHub Models | Ollama |
|----------|------------|---------------|--------|
| General Chat | `gpt-5.1` | `openai/gpt-4.1` | `llama3.1:70b` |
| Coding | `gpt-5.2-codex` | `openai/gpt-4.1` | `qwen2.5-coder:32b` |
| Reasoning | `o3` | `openai/o3` | `deepseek-r1:70b` |
| Fast/Cheap | `gpt-5-mini` | `openai/gpt-4o-mini` | `phi4-mini` |
| Embeddings | `text-embedding-3-large` | `openai/text-embedding-3-large` | `nomic-embed-text` |
| Vision | `gpt-5` | `openai/gpt-4o` | `llama3.2-vision` |
| Audio | `gpt-audio` | - | - |
| Local/Privacy | - | - | `deepseek-r1` or `llama3.1` |

---

## Sources

### Primary Sources
1. [OpenAI Models Documentation](https://platform.openai.com/docs/models) - Official model catalog
2. [OpenAI Deprecations Page](https://developers.openai.com/api/docs/deprecations/) - Official deprecation schedule
3. [GitHub Models Catalog API](https://docs.github.com/en/rest/models/catalog) - GitHub Models API documentation
4. [Ollama Library](https://ollama.com/library) - Official Ollama model library

### Secondary Sources
1. [GitHub Models Marketplace](https://github.com/marketplace?type=models) - Browse available models
2. [Ollama Models List 2025 - Skywork.ai](https://skywork.ai/blog/llm/ollama-models-list-2025-100-models-compared/) - Comprehensive comparison
3. [Open-Source LLMs 2025 - n8n Blog](https://blog.n8n.io/open-source-llm/) - Best open-source models review

---

## Related Topics

- Microsoft.Extensions.AI integration patterns for multi-provider support
- Model versioning and aliasing best practices
- Cost optimization strategies for production deployments
- Local vs cloud model selection criteria
