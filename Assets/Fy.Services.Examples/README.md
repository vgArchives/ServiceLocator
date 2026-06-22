# Examples

These are small, working services that show how to use the ServiceLocator in a
real project. Each folder is one service plus a little example component you can
drop on a GameObject to watch it run. Read whichever one is closest to what
you're building and copy from there.

Nothing here is needed at runtime by the package itself. It's all reference
material, safe to delete once you've got the hang of it.

## How to run one

Make an empty GameObject in a scene, add one of the `*Example` components to it
(for instance `InventoryServiceExample`), and press Play. Watch the Console.
That's it. There's no setup step and no scene to open.

## What each one shows

| Folder | Pattern | When you'd use it |
|--------|---------|-------------------|
| Inventory | Regular C# service, resolved with `TryGet` | The default. A normal class, no registration needed. |
| Sfx | MonoBehaviour service | The service needs to live on a GameObject (AudioSource, coroutines, Update). |
| Difficulty | `[DynamicService]` | You need to swap the implementation at runtime. |
| Save | `[RequiredService]` + `GetChecked` | The game can't run without it, so a failure should throw, not return null. |
| Localization | `[PreloadService]` | You want it built during load instead of on first use. |
| Spawn | `[AbstractService]` base + concrete | A shared base contract plus a concrete service you actually resolve. |

If you're not sure where to start, start with Inventory. It covers the case
you'll hit most often.

## Writing your own service

The shortest path is three steps:

1. Write an interface that inherits `IService`.
2. Write a class that implements it.
3. Ask for it with `ServiceLocator.TryGet<IYourService>(out var service)`.

The locator notices your type, creates it the
first time it's asked for, and reuses that instance after. The attributes in the
table above are extras for the most common cases, and each example
explains the one it uses right at the top of the file.

## A service that starts with the game

By default a service isn't built until the first time something asks for it. If
you'd rather it be ready up front, before any gameplay runs, put
`[PreloadService]` on the implementation class:

```csharp
[PreloadService]
public sealed class LocalizationService : ILocalizationService
{
    public LocalizationService()
    {
        // load tables, warm caches, whatever you need ready on day one
    }

    // ...
}
```

The locator creates the instance before
the first scene loads, so the cost is paid during loading instead of as a hitch
mid-game, and it's already there the first time you `TryGet` it. Reach for this
when building the service is expensive and you know you'll need it anyway, like
reading localization off disk. The Localization folder has a complete example.

## A MonoBehaviour service that survives scene loads

Regular C# services already stick around when you change scenes, because the
locator holds onto them. MonoBehaviour services don't get that for free: their
GameObject is destroyed when the scene unloads, and the next time you ask for
the service a fresh one is built. That's usually fine, but sometimes you want
the same instance (and its state) to live for the whole game, like an audio
manager that shouldn't restart between scenes.

Put `[PersistentService]` on the MonoBehaviour implementation:

```csharp
[PersistentService]
public sealed class MusicService : MonoBehaviour, IMusicService
{
    // ...
}
```

When the locator creates the service it calls `DontDestroyOnLoad` on its
GameObject, so it carries over from one scene to the next instead of being
rebuilt. One thing to watch: if you place the service in a scene by hand, keep
it at the scene root. `DontDestroyOnLoad` only works on root objects, and a
service sitting as a child of something else can't persist.

This is for MonoBehaviour services only. On a regular C# service the attribute
does nothing useful (they already persist), and the locator will log an error to
let you know.
