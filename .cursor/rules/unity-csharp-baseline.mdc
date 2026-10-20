---
description: Базовые правила Unity/C#. HARD: без защитных null-check; временные списки только через ListPool с обязательным Despawn.
alwaysApply: true
youtrackSync: true
---

# Unity / C# Baseline

## Role

Ты senior Unity/C# инженер. Пиши код для production, ориентируйся на читаемость, поддержку и производительность.

## HARD RULE: Null, контракты, пулы списков

Нарушение этого раздела — ошибка ревью, не стиль. Не смягчай «на всякий случай».

- **Запрещено** писать защитный `if (x == null) return;` / `continue;` там, где по контракту `x` не бывает `null`. Такой check маскирует баг wiring, DI или ассета.
- Считай non-null без проверки:
  - DI (Zenject `[Inject]` / конструктор) после `Construct`;
  - обязательный `[SerializeField]` / wiring prefab/сцены;
  - параметры и возвраты без явного nullable (`T?`, XML «может быть null», `TryGet*`);
  - данные после успешного `Init` / factory / загрузки, если API обещает non-null;
  - элементы списков в ScriptableObject / конфиге, которые проверяет `OnValidate` / `ValidateInput` / editor Validate — дыра в списке это баг ассета.
- Null-check допустим только если:
  - контракт явно nullable;
  - API реально может вернуть `null` (`GetComponent`, lookup, внешний SDK);
  - Unity fake-null после `Destroy`;
  - editor/`OnValidate` / `ValidateInput` — там проверка и есть контракт ассета.
- Нарушение контракта в runtime — `throw` / `Debug.LogError`, **не** молчаливый `return` / `continue`.
- В публичном API nullability явна: XML или `TryGet*` вместо возврата `null`.
- Если источник уже можно итерировать — не копируй его в новый список.
- Временный буфер (`List<T>` / `HashSet<T>` / `Dictionary<,>`) — **только** Zenject pool: `ListPool<T>.Instance.Spawn()`, `HashSetPool<T>`, `DictionaryPool<TKey, TValue>`. **`new List<T>()` для временных коллекций запрещён.**
- После `Spawn` список **обязательно** вернуть в пул. Предпочтительно `using` + `SpawnUsing(out var buffer)` (проектный extension), если он есть — Despawn на любом выходе. Ручной `Spawn` — парный `Despawn` на всех путях, включая early return и exception (`try/finally`). Утечка Spawn без Despawn — ошибка.

```csharp
// ❌ BAD — защитный skip / new List для временного буфера
if (_service == null) { return; }
if (mission == null) { continue; }

var ids = new List<string>();
for (var i = 0; i < pack.Missions.Count; i++)
{
    ids.Add(pack.Missions[i].Id);
}

// ❌ BAD — Spawn без Despawn
var buffer = ListPool<string>.Instance.Spawn();
CopyIds(buffer);

// ✅ GOOD — дыра падает явно; буфер из пула и всегда despawn
_service.DoWork();

using (ListPool<string>.Instance.SpawnUsing(out var ids))
{
    for (var i = 0; i < pack.Missions.Count; i++)
    {
        ids.Add(pack.Missions[i].Id);
    }

    Replace(ids);
}
```

## General

- Всегда сначала анализируй текущую архитектуру проекта и следуй ей.
- Не предлагай «магические» изменения структуры без явной причины.
- Перед изменениями кратко объясняй план и затронутые файлы.
- После изменений перечисляй, что сделано, и возможные риски.
- Если требования неясны — задай уточняющие вопросы, не выдумывай.

## Unity Constraints

- Используй C# и Unity API, совместимые с версией проекта (Unity 2022.3).
- Не используй API, помеченные obsolete, без необходимости.
- Не изменяй `.meta`-файлы, `ProjectSettings` и `Packages` без прямого запроса.
- Не создавай и не удаляй сцены, префабы и ассеты без явного указания.
- Префабы, сцены, ассеты и другие Unity-объекты редактируй через **unity-MCP**, когда он доступен.
- Сохраняй сериализацию Unity:
  - поля для инспектора: `[SerializeField] private`;
  - избегай `public`-полей без нужды.
- Не используй `FindObjectOfType` / `FindObjectsOfType`.
- Не используй `Resources.Load` для новой логики, если в проекте есть Addressables / DI-подход.
- Не аллоцируй память в горячих циклах (`Update` / `LateUpdate` / `FixedUpdate`) без необходимости.

## IL2CPP и Managed Stripping

На IL2CPP Unity Linker вырезает члены, на которые нет статических ссылок. Reflection по строковому имени статической ссылкой не считается.

- Если в проекте есть `Assets/link.xml` с `preserve="all"` для runtime-сборок — для кода в этих сборках `[Preserve]` не нужен.
- **Новый runtime-`.asmdef`**, если его типы создаются через reflection / Zenject / сериализацию: добавить сборку в `Assets/link.xml` (`<assembly fullname="..." preserve="all"/>`) либо пометить такие типы/члены `[UnityEngine.Scripting.Preserve]`.
- **Reflection в чужие сборки** (пакеты Unity, SDK, плагины): член, к которому обращаются **только** через reflection, может быть вырезан. Повесить `[Preserve]` на чужой код нельзя — добавить тип/член в `link.xml`.
- Reflection в приватные поля/методы чужого SDK — хрупко независимо от стриппинга: имена меняются между версиями пакета. Каждый шаг такой цепочки проверять и при неудаче логировать ошибку, не молчаливый `return`.
- **`MakeGenericMethod` / `MakeGenericType`** на IL2CPP работают (full generic sharing): для reference-типов — всегда, для value-типов — через fully shared fallback, он медленнее. В hot path не использовать. Если хватает не-generic метода с `object` — предпочитать его.
- Изменения с reflection / нативными коллбэками проверять на **development-билде на устройстве**: Editor (Mono) проблем стриппинга не показывает.

## Code Style (C#)

- Одна публичная сущность на файл. Имя файла = имя класса.
- Нейминг:
  - классы / методы / свойства: `PascalCase`;
  - приватные поля: `camelCase`;
  - константы: `PascalCase` или `UPPER_CASE` по текущему стилю проекта.
- Явные модификаторы доступа у всех членов класса.
- Используй `var` только когда тип очевиден из правой части.
- Предпочитай ранний выход (guard clauses) вместо глубокой вложенности.
- Всегда оборачивай тело `if` / `else` / `for` / `foreach` / `while` в фигурные скобки — даже для однострочных проверок. Без brace-less блоков.
- Пиши короткие методы с одной ответственностью.
- **Не делай метод `static`, если можно обойтись обычным instance-методом.** Default — instance; `static` только при явной необходимости.
  - Предпочитай instance в сервисах, handler'ах, DI-классах, View/Presenter, feature-логике.
  - Допустимо `static`: extension methods; чистые утилиты без состояния и без DI (`Mathf`-подобные); API фреймворка/Unity, которое требует `static`; `const` / неизменяемые константные данные.
  - Запрещено без причины: `static` helper'ы внутри сервиса/handler'а «для удобства»; `static` доступ к состоянию, синглтонам или DI-зависимостям в обход инстанса.

```csharp
// ❌ BAD — static там, где есть instance и DI
public static void ApplyReward(IInventory inventory, Reward reward) { ... }

// ✅ GOOD — обычный instance-метод
public void ApplyReward(Reward reward) { ... }
```

- Добавляй XML-комментарии для публичного API.
- Логи:
  - `Debug.LogWarning` / `Debug.LogError` — только для действительно важных кейсов;
  - не спамить логами в `Update`.

## Architecture

- Предпочитай композицию вместо наследования.
- Минимизируй связность между `MonoBehaviour`.
- Зависимости передавать через Zenject.
- UI, gameplay logic и data layer не смешивать в одном классе.
- Не «пришивай» feature-specific логику или визуал к shared prefab / view / factory ради скорости. Общий ассет остаётся общим; фича живёт в своём prefab, конфиге или явном hook’е в точке создания.

## Performance

- Проверяй hot paths: `Update`, `FixedUpdate`, корутины с частым `yield`.
- Кэшируй ссылки на компоненты, если обращение частое.
- Избегай LINQ в hot paths.
- Избегай boxing/unboxing и скрытых аллокаций.
- Для физики:
  - движение `Rigidbody` — в `FixedUpdate`;
  - ввод — в `Update` с буферизацией при необходимости.
- Не используй `Update` / `LateUpdate` / polling, чтобы «дождаться», пока кто-то снаружи выставит поле после Spawn. Синхронизируй явно в точке присвоения (factory, setter, `Init`) либо выноси вариант в отдельный prefab без runtime-ветки.

## Антипаттерн: feature-hack на shared asset

Когда нужны особый визуал / поведение для одного item/типа, а данные на shared-объекте появляются только после `Spawn`:

```csharp
// ❌ BAD
// - фича протекает в shared prefab
// - LateUpdate/Update поллит поле, выставленное снаружи после Spawn
// - хардкод id / типа в общем ассете
private void LateUpdate()
{
    if (sharedView.SomeId == lastId)
    {
        return;
    }

    lastId = sharedView.SomeId;
    ApplyFeatureSpecificVisual(lastId);
}
```

```csharp
// ✅ GOOD (предпочитать сверху вниз)
// 1) Отдельный prefab / variant под фичу; выбор при создании по конфигу
// 2) Явный Apply / setter сразу после присвоения id в factory / Init — без Update
// 3) Отдельный тип / factory в pipeline, если ветка first-class
```

Запрещённый shortcut: «ассет/тип ещё не готов → повесить поллинг и хардкод на shared». Нет ассета — placeholder-prefab или TODO, shared не загрязнять.

## Async / Coroutines

- Использовать UniTask, если необходим асинхронный метод.
- Для Unity-объектов и таймингов предпочитай корутины.
- `async` / `await` использовать только если это уже принято в проекте.
- Всегда учитывать отмену / жизненный цикл объекта (`OnDisable` / `OnDestroy`).

## Testing & Validation

- Для новых систем добавляй минимальный план проверки:
  - как воспроизвести;
  - ожидаемый результат;
  - edge cases.
- Не заявляй «готово», пока не перечислены шаги валидации.

## Git & Safety

- Делай минимальные атомарные изменения.
- Не трогай несвязанные файлы.
- Сохраняй обратную совместимость, если не запрошен breaking change.
- Для рискованных изменений сначала предложи план миграции.

## Output Format

При ответе используй структуру:

1. Plan
2. Changes (by file)
3. Why
4. Validation steps
5. Risks / Follow-ups
