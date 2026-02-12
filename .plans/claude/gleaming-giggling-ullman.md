# Plan: Update Model Configurations to Current 2025-2026 Models

## Context

The current `ModelConfiguration` enum and documentation use outdated models (GPT-4o variants from 2024). Research shows:
- **OpenAI**: GPT-5 series is now flagship; GPT-4o and o1-series are being deprecated throughout 2025-2026
- **GitHub Models**: Now offers GPT-4.1, o3, o4-mini, and Phi-4 series
- **Ollama**: DeepSeek-R1, Llama 3.1/3.2, and Phi-4 are the most popular current models

## Files to Modify

| File | Purpose |
|------|---------|
| `AiDevs4/src/AiClients/ModelConfigurations.cs` | Replace model enum values and mappings |
| `AiDevs4/src/docs/providers/README.md` | Update documentation table |

## Proposed New Models

### OpenAI API (3 models)

| Enum Value | Model ID | Notes |
|------------|----------|-------|
| `Gpt5` | gpt-5 | Current flagship |
| `Gpt5_Mini` | gpt-5-mini | Cost-efficient |
| `Gpt4_1` | gpt-4.1 | Stable non-reasoning |

### GitHub Models (5 models)

| Enum Value | Model ID | Notes |
|------------|----------|-------|
| `Gpt4_1_Github` | gpt-4.1 | Latest GPT-4 |
| `Gpt4_1_Mini_Github` | gpt-4.1-mini | Cost-efficient |
| `O3_Mini_Github` | o3-mini | Reasoning |
| `O4_Mini_Github` | o4-mini | Latest reasoning |
| `Phi4_Github` | phi-4 | Microsoft lightweight |

### Ollama (3 models)

| Enum Value | Model ID | Notes |
|------------|----------|-------|
| `OllamaLlama31` | llama3.1 | Most popular (110M pulls) |
| `OllamaDeepSeekR1` | deepseek-r1 | Reasoning (78M pulls) |
| `OllamaPhi4` | phi4 | Microsoft lightweight |

## Implementation Steps

1. **Update `ModelConfigurations.cs`**:
   - Replace all enum values with new models (no backward compatibility)
   - Update `s_modelMappings` dictionary with new model IDs
   - Remove GPT-4o entries entirely

2. **Update `README.md`**:
   - Replace the outdated model table with current models
   - Update usage example to use `Gpt4_1_Mini_Github` as default

## Verification

- `dotnet build` compiles without errors
- `dotnet run` starts successfully
- Swagger UI shows the updated model options in `/answer` endpoints
