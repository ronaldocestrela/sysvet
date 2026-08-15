# `src/Clients/SharedUI/` — Biblioteca de Componentes Razor Compartilhados

**Razor Class Library (RCL)** que serve como a única fonte de verdade para componentes de UI do SysVet. Tanto o `BlazorWeb` quanto o `MauiApp` referenciam este projeto — garantindo paridade visual e zero duplicação de código de interface.

## Arquivos Atuais

| Arquivo | O que é / Para que serve |
|---|---|
| [`SharedUI.csproj`](./SharedUI.csproj) | Projeto do tipo `Razor Class Library`. Define que esta biblioteca expõe componentes Razor e arquivos estáticos via `wwwroot/`. |
| [`_Imports.razor`](./_Imports.razor) | Importações de namespace globais para todos os componentes da biblioteca. |
| [`Component1.razor`](./Component1.razor) | **Componente de placeholder** gerado pelo template. Deve ser substituído pelos primeiros componentes reais do sistema. |
| [`Component1.razor.css`](./Component1.razor.css) | CSS com escopo isolado do `Component1`. |
| [`ExampleJsInterop.cs`](./ExampleJsInterop.cs) | **Exemplo de interop JS** gerado pelo template. Demonstra como chamar funções JavaScript a partir de C# via `IJSRuntime`. Remover antes da produção. |
| [`wwwroot/`](./wwwroot/) | Arquivos estáticos exportados pela biblioteca (CSS global, fontes, imagens, scripts JS). |

## `wwwroot/`

| Arquivo | Propósito |
|---|---|
| `background.png` | Imagem de exemplo do template. Remover antes da produção. |
| `exampleJsInterop.js` | Arquivo JS de exemplo para interop. Referenciado por `ExampleJsInterop.cs`. Remover antes da produção. |

## Estrutura Futura Esperada

À medida que o sistema crescer, esta pasta abrigará os componentes reais:

```
SharedUI/
├── Components/
│   ├── Forms/          ← inputs, dropdowns, datepickers customizados
│   ├── Layout/         ← headers, sidebars, cards, modais
│   └── Tables/         ← tabelas de dados, paginação
├── Services/           ← estado de UI (ex: AuthStateProvider, ThemeService)
├── wwwroot/
│   └── css/            ← design tokens, variáveis CSS globais
└── _Imports.razor
```

> **Regra:** Nenhum componente visual deve existir exclusivamente em `BlazorWeb/` ou `MauiApp/`. Todo componente reutilizável pertence aqui.
