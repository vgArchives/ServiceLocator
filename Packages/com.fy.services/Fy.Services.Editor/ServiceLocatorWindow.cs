using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fy.Services.Editor
{
    /// <summary>
    /// Editor window that lists the registered services and their state. Open it from Window/Fy/Service Locator.
    /// </summary>
    public sealed class ServiceLocatorWindow : EditorWindow
    {
        private const BindingFlags MemberFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static readonly BadgeStyle[] BadgeStyles =
        {
            new(typeof(AbstractServiceAttribute), "Abstract", new Color(0.55f, 0.57f, 0.60f),
                "[AbstractService] \u2014 interface hidden from auto-registration."),
            new(typeof(DynamicServiceAttribute), "Dynamic", new Color(0.32f, 0.82f, 0.69f),
                "[DynamicService] \u2014 SetService may replace and dispose the previous instance."),
            new(typeof(RequiredServiceAttribute), "Required", new Color(0.76f, 0.87f, 0.42f),
                "[RequiredService] \u2014 GetChecked throws when the service is missing."),
            new(typeof(PreloadServiceAttribute), "Preload", new Color(0.12f, 0.62f, 0.80f),
                "[PreloadService] \u2014 resolved at BeforeSceneLoad."),
            new(typeof(DisableDefaultFactoryAttribute), "DisableDefaultFactory", new Color(0.64f, 0.56f, 0.80f),
                "[DisableDefaultFactory] \u2014 no default factory is registered for this class."),
            new(typeof(PersistentServiceAttribute), "Persistent", new Color(0.90f, 0.62f, 0.36f),
                "[PersistentService] \u2014 MonoBehaviour service survives scene loads (DontDestroyOnLoad).")
        };

        private static readonly Color InitPreloadedColor = new(0.12f, 0.62f, 0.80f);
        private static readonly Color InitLazyColor = new(0.90f, 0.72f, 0.32f);

        private const float Space1 = 4f;
        private const float Space2 = 8f;
        private const float CardRadius = 5f;

        private const int RunningTabIndex = 0;

        private TabView _rootTabView;
        private AssemblyTabSection _runningSection;
        private AssemblyTabSection _interfacesSection;
        private AssemblyTabSection _classesSection;

        private static bool IsDark => EditorGUIUtility.isProSkin;

        private static Color CardBackgroundColor => IsDark ? new Color(1f, 1f, 1f, 0.035f) : new Color(0f, 0f, 0f, 0.02f);
        private static Color CardBorderColor => IsDark ? new Color(0f, 0f, 0f, 0.45f) : new Color(0f, 0f, 0f, 0.16f);
        private static Color MutedTextColor => IsDark ? new Color(1f, 1f, 1f, 0.72f) : new Color(0f, 0f, 0f, 0.6f);
        private static Color SeparatorColor => IsDark ? new Color(1f, 1f, 1f, 0.09f) : new Color(0f, 0f, 0f, 0.09f);

        [MenuItem("Window/Fy/Service Locator")]
        public static void Open()
        {
            ServiceLocatorWindow window = GetWindow<ServiceLocatorWindow>();
            window.titleContent = new GUIContent("Service Locator");
        }

        private void CreateGUI()
        {
            rootVisualElement.style.fontSize = 13;

            _rootTabView = new TabView();
            _rootTabView.style.flexGrow = 1;

            _runningSection = new AssemblyTabSection(CreateTabHost(_rootTabView, "Running Services"));
            _interfacesSection = new AssemblyTabSection(CreateTabHost(_rootTabView, "Service Interfaces"));
            _classesSection = new AssemblyTabSection(CreateTabHost(_rootTabView, "Service Classes"));

            rootVisualElement.Add(_rootTabView);

            BuildInterfaces();
            BuildClasses();

            if (Application.isPlaying)
            {
                RebuildRunningServices();
            }
            else
            {
                ShowRunningPlaceholder();
            }

            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            rootVisualElement.schedule.Execute(Refresh).Every(0);
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        }

        private void HandlePlayModeStateChanged(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.EnteredEditMode:
                {
                    ShowRunningPlaceholder();

                    break;
                }

                case PlayModeStateChange.EnteredPlayMode:
                {
                    BuildInterfaces();
                    BuildClasses();

                    break;
                }
            }
        }

        private void Refresh()
        {
            if (!Application.isPlaying || _rootTabView.selectedTabIndex != RunningTabIndex)
            {
                return;
            }

            RebuildRunningServices();
        }

        private void RebuildRunningServices()
        {
            List<ServiceSnapshot> snapshots = ServiceLocator.EnumerateServices()
                .Where(snapshot => snapshot.Value.IsValid())
                .OrderBy(snapshot => snapshot.InterfaceType.Name)
                .ToList();

            if (snapshots.Count == 0)
            {
                _runningSection.ShowMessage("No services are running.");

                return;
            }

            _runningSection.Refresh(snapshots, snapshot => AssemblyNameOf(snapshot.InterfaceType),
                RunningSignature, CreateRunningCard);
        }

        private static string RunningSignature(ServiceSnapshot snapshot)
        {
            Type concreteType = ResolveConcreteType(snapshot);
            bool resolved = snapshot.Value.IsValid();
            int identity = resolved ? RuntimeHelpers.GetHashCode(snapshot.Value) : 0;

            return $"{snapshot.InterfaceType.FullName}|{concreteType?.FullName}|{resolved}|{identity}|" +
                   $"{snapshot.Factory?.GetType().FullName}|{snapshot.IsDynamic}|{snapshot.IsRequired}";
        }

        private void ShowRunningPlaceholder()
        {
            _runningSection.ShowMessage("Running services appear in Play Mode.");
        }

        private void BuildInterfaces()
        {
            List<Type> interfaces = TypeCache.GetTypesDerivedFrom<IService>()
                .Where(type => type.IsInterface && type != typeof(IService))
                .OrderBy(type => type.Name)
                .ToList();

            if (interfaces.Count == 0)
            {
                _interfacesSection.ShowMessage("No service interfaces found.");

                return;
            }

            _interfacesSection.Refresh(interfaces, AssemblyNameOf, type => type.FullName,
                type => new Card(CreateInterfaceCard(type), null));
        }

        private void BuildClasses()
        {
            List<Type> classes = TypeCache.GetTypesDerivedFrom<IService>()
                .Where(type => !type.IsInterface)
                .OrderBy(type => type.Name)
                .ToList();

            if (classes.Count == 0)
            {
                _classesSection.ShowMessage("No service classes found.");

                return;
            }

            _classesSection.Refresh(classes, AssemblyNameOf, type => type.FullName,
                type => new Card(CreateClassCard(type), null));
        }

        private static VisualElement CreateTabHost(TabView tabView, string label)
        {
            Tab tab = new Tab(label);
            VisualElement host = new VisualElement();
            host.style.flexGrow = 1;
            tab.Add(host);
            tabView.Add(tab);

            return host;
        }

        private static string AssemblyNameOf(Type type)
        {
            return type.Assembly.GetName().Name;
        }

        private static Card CreateRunningCard(ServiceSnapshot snapshot)
        {
            VisualElement card = CreateCard();
            Type concreteType = ResolveConcreteType(snapshot);
            (Color initColor, string initLabel) = DescribeInitialization(concreteType);

            card.Add(CreateRunningHeader(snapshot.InterfaceType.Name,
                concreteType != null ? concreteType.Name : "unknown", initColor, initLabel));
            card.Add(CreateAttributesRow(snapshot.InterfaceType, concreteType));
            card.Add(CreateKeyValueRow("Source", DescribeFactory(snapshot.Factory)));

            VisualElement wrapper = CreateSubSection("Wrapper");

            if (snapshot.Value is UnityEngine.Object unityValue)
            {
                wrapper.Add(CreateElementRow("Value", CreateReadOnlyObjectField(unityValue.GetType(), unityValue)));
            }
            else
            {
                wrapper.Add(CreateKeyValueRow("Value", snapshot.Value.IsValid() ? snapshot.Value.GetType().Name : "null"));
            }

            wrapper.Add(CreateKeyValueRow("Factory", snapshot.Factory != null ? snapshot.Factory.GetType().Name : "null"));
            card.Add(wrapper);

            Action update = snapshot.Value.IsValid() ? AddMarkedMembers(card, snapshot.Value) : null;

            return new Card(card, update);
        }

        private static VisualElement CreateInterfaceCard(Type interfaceType)
        {
            VisualElement card = CreateCard();

            card.Add(CreateTypeCardHeader("Service Interface", interfaceType.Name, interfaceType.Namespace));
            card.Add(CreateAttributesRow(interfaceType));

            List<Type> implementations = TypeCache.GetTypesDerivedFrom(interfaceType)
                .Where(type => type is { IsInterface: false, IsAbstract: false })
                .OrderBy(type => type.Name)
                .ToList();

            VisualElement section = CreateSubSection($"Implementations ({implementations.Count})");

            if (implementations.Count == 0)
            {
                section.Add(CreateInfoLabel("(none)"));
            }
            else
            {
                foreach (Type implementation in implementations)
                {
                    section.Add(CreateInfoLabel(implementation.Name));
                }
            }

            card.Add(section);

            return card;
        }

        private static VisualElement CreateClassCard(Type classType)
        {
            VisualElement card = CreateCard();

            card.Add(CreateTypeCardHeader("Class Implementation", classType.Name, classType.Namespace));
            card.Add(CreateAttributesRow(classType));
            card.Add(CreateKeyValueRow("Abstract", classType.IsAbstract.ToString()));
            card.Add(CreateKeyValueRow("MonoBehaviour", typeof(MonoBehaviour).IsAssignableFrom(classType).ToString()));

            List<Type> serviceInterfaces = classType.GetInterfaces()
                .Where(type => type != typeof(IService) && typeof(IService).IsAssignableFrom(type))
                .OrderBy(type => type.Name)
                .ToList();

            VisualElement section = CreateSubSection($"Service Interfaces ({serviceInterfaces.Count})");

            if (serviceInterfaces.Count == 0)
            {
                section.Add(CreateInfoLabel("(none)"));
            }
            else
            {
                foreach (Type serviceInterface in serviceInterfaces)
                {
                    section.Add(CreateInfoLabel(serviceInterface.Name));
                }
            }

            card.Add(section);

            return card;
        }

        private static Action AddMarkedMembers(VisualElement card, IService instance)
        {
            List<(string Name, Type Type, Func<object> Getter)> members = GetMarkedMembers(instance).ToList();

            if (members.Count == 0)
            {
                return null;
            }

            VisualElement section = CreateSubSection("Inspected Fields");
            List<Action> updaters = new();

            foreach ((string name, Type type, Func<object> getter) in members)
            {
                (VisualElement element, Action update) = CreateValueView(type, getter);
                section.Add(CreateMemberRow(name, element));
                updaters.Add(update);
            }

            card.Add(section);

            return () =>
            {
                foreach (Action update in updaters)
                {
                    update();
                }
            };
        }

        private static IEnumerable<(string Name, Type Type, Func<object> Getter)> GetMarkedMembers(object instance)
        {
            Type type = instance.GetType();

            foreach (FieldInfo field in type.GetFields(MemberFlags))
            {
                if (field.GetCustomAttribute<ShowInServiceWindowAttribute>() != null)
                {
                    yield return (field.Name, field.FieldType, () => field.GetValue(instance));
                }
            }

            foreach (PropertyInfo property in type.GetProperties(MemberFlags))
            {
                if (!property.CanRead
                 || property.GetIndexParameters().Length != 0
                 || property.GetCustomAttribute<ShowInServiceWindowAttribute>() == null)
                {
                    continue;
                }

                yield return (property.Name, property.PropertyType, () => SafeGet(property, instance));
            }
        }

        private static object SafeGet(PropertyInfo property, object instance)
        {
            try
            {
                return property.GetValue(instance);
            }
            catch (Exception exception)
            {
                return $"<error: {exception.Message}>";
            }
        }

        private static Type ResolveConcreteType(ServiceSnapshot snapshot)
        {
            if (snapshot.Value.IsValid())
            {
                return snapshot.Value.GetType();
            }

            if (snapshot.Factory == null)
            {
                return null;
            }

            Type factoryType = snapshot.Factory.GetType();

            if (factoryType.IsGenericType)
            {
                Type definition = factoryType.GetGenericTypeDefinition();

                if (definition == typeof(DefaultServiceFactory<>) || definition == typeof(DefaultServiceActorFactory<>))
                {
                    return factoryType.GetGenericArguments()[0];
                }
            }

            return null;
        }

        private static (Color Color, string Label) DescribeInitialization(Type concreteType)
        {
            bool isPreloaded = concreteType != null
                            && concreteType.GetCustomAttribute<PreloadServiceAttribute>() != null;

            return isPreloaded
                ? (InitPreloadedColor, "Preloaded")
                : (InitLazyColor, "Lazy Instantiated");
        }

        private static string DescribeFactory(IServiceFactory factory)
        {
            if (factory == null)
            {
                return "Manual (SetService)";
            }

            Type factoryType = factory.GetType();

            if (factoryType.IsGenericType)
            {
                Type definition = factoryType.GetGenericTypeDefinition();

                if (definition == typeof(DefaultServiceFactory<>))
                {
                    return "Default factory (new)";
                }

                if (definition == typeof(DefaultServiceActorFactory<>))
                {
                    return "Default actor factory (MonoBehaviour)";
                }
            }

            return $"Custom factory ({factoryType.Name})";
        }

        private static VisualElement CreateMemberRow(string name, VisualElement valueElement)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginTop = 1;

            Label nameLabel = new Label(name);
            nameLabel.style.minWidth = 120;
            nameLabel.style.color = MutedTextColor;
            nameLabel.style.fontSize = 12;
            row.Add(nameLabel);

            valueElement.style.flexGrow = 1;
            row.Add(valueElement);

            return row;
        }

        private static VisualElement CreateKeyValueRow(string key, string value)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginTop = 1;

            Label keyLabel = new Label(key);
            keyLabel.style.minWidth = 104;
            keyLabel.style.color = MutedTextColor;
            keyLabel.style.fontSize = 12;
            row.Add(keyLabel);

            Label valueLabel = new Label(value);
            valueLabel.style.flexGrow = 1;
            valueLabel.style.whiteSpace = WhiteSpace.Normal;
            valueLabel.selection.isSelectable = true;
            row.Add(valueLabel);

            return row;
        }

        private static VisualElement CreateElementRow(string key, VisualElement element)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginTop = 1;

            Label keyLabel = new Label(key);
            keyLabel.style.minWidth = 104;
            keyLabel.style.color = MutedTextColor;
            keyLabel.style.fontSize = 12;
            row.Add(keyLabel);

            element.style.flexGrow = 1;
            row.Add(element);

            return row;
        }

        private static (VisualElement Element, Action Update) CreateValueView(Type memberType, Func<object> getter)
        {
            object initial = getter();

            if (typeof(UnityEngine.Object).IsAssignableFrom(memberType) || initial is UnityEngine.Object)
            {
                ObjectField objectField = CreateReadOnlyObjectField(ResolveObjectType(memberType, initial),
                    initial as UnityEngine.Object);

                return (objectField, () => objectField.SetValueWithoutNotify(getter() as UnityEngine.Object));
            }

            if (memberType == typeof(bool))
            {
                Toggle toggle = new Toggle();
                toggle.SetEnabled(false);
                toggle.SetValueWithoutNotify(initial is true);

                return (toggle, () => toggle.SetValueWithoutNotify(getter() is true));
            }

            if (memberType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(memberType))
            {
                Foldout foldout = new Foldout { value = false };
                PopulateCollection(foldout, initial);

                return (foldout, () => PopulateCollection(foldout, getter()));
            }

            Label valueLabel = CreateValueLabel(FormatScalar(initial));

            return (valueLabel, () => valueLabel.text = FormatScalar(getter()));
        }

        private static void PopulateCollection(Foldout foldout, object value)
        {
            foldout.Clear();
            int count = 0;

            if (value is IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                {
                    foldout.Add(CreateValueLabel($"[{count}] {FormatScalar(item)}"));
                    count++;
                }
            }

            if (count == 0)
            {
                foldout.Add(CreateValueLabel("(empty)"));
            }

            foldout.text = $"Items ({count})";
        }

        private static Type ResolveObjectType(Type memberType, object value)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(memberType))
            {
                return memberType;
            }

            return value?.GetType() ?? typeof(UnityEngine.Object);
        }

        private static ObjectField CreateReadOnlyObjectField(Type objectType, UnityEngine.Object value)
        {
            ObjectField field = new ObjectField { objectType = objectType };
            field.tooltip = "Click to select and ping it in the scene.";
            field.SetValueWithoutNotify(value);
            field.Q(className: "unity-object-field__selector")?.SetEnabled(false);
            field.RegisterValueChangedCallback(changeEvent => field.SetValueWithoutNotify(changeEvent.previousValue));

            return field;
        }

        private static string FormatScalar(object value)
        {
            return value != null ? value.ToString() : "null";
        }

        private static Label CreateValueLabel(string text)
        {
            Label label = new Label(text);
            label.selection.isSelectable = true;
            label.style.whiteSpace = WhiteSpace.Normal;

            return label;
        }

        private static VisualElement CreateAttributesRow(params Type[] typesToScan)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.flexWrap = Wrap.Wrap;
            row.style.marginTop = Space1;

            Label prefixLabel = new Label("Custom Attributes:");
            prefixLabel.style.color = MutedTextColor;
            prefixLabel.style.fontSize = 11;
            prefixLabel.style.marginRight = Space1;
            row.Add(prefixLabel);

            int added = 0;

            foreach (BadgeStyle badge in BadgeStyles)
            {
                if (HasAttribute(badge.AttributeType, typesToScan))
                {
                    row.Add(CreateBadge(badge));
                    added++;
                }
            }

            if (added == 0)
            {
                Label none = new Label("None");
                none.style.color = MutedTextColor;
                none.style.fontSize = 11;
                row.Add(none);
            }

            return row;
        }

        private static bool HasAttribute(Type attributeType, Type[] typesToScan)
        {
            foreach (Type type in typesToScan)
            {
                if (type != null && type.GetCustomAttribute(attributeType) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static VisualElement CreateBadge(BadgeStyle badge)
        {
            Label label = new Label(badge.Label);
            label.tooltip = badge.Tooltip;
            label.style.color = Saturated(badge.Color);
            label.style.backgroundColor = new Color(badge.Color.r, badge.Color.g, badge.Color.b, 0.22f);
            label.style.fontSize = 11;
            label.style.paddingLeft = 6;
            label.style.paddingRight = 6;
            label.style.paddingTop = 1;
            label.style.paddingBottom = 1;
            label.style.marginRight = Space1;
            label.style.marginTop = 2;

            Color borderColor = new Color(badge.Color.r, badge.Color.g, badge.Color.b, 0.4f);
            label.style.borderTopWidth = 1;
            label.style.borderBottomWidth = 1;
            label.style.borderLeftWidth = 1;
            label.style.borderRightWidth = 1;
            label.style.borderTopColor = borderColor;
            label.style.borderBottomColor = borderColor;
            label.style.borderLeftColor = borderColor;
            label.style.borderRightColor = borderColor;
            SetBorderRadius(label, 3);

            return label;
        }

        private static VisualElement CreateCard()
        {
            VisualElement card = new VisualElement();
            card.style.marginTop = Space1;
            card.style.marginBottom = Space2;
            card.style.marginLeft = Space2;
            card.style.marginRight = Space2;
            card.style.paddingTop = Space2;
            card.style.paddingBottom = Space2;
            card.style.paddingLeft = Space2;
            card.style.paddingRight = Space2;
            card.style.backgroundColor = CardBackgroundColor;
            SetBorderRadius(card, CardRadius);

            Color borderColor = CardBorderColor;
            card.style.borderTopWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderTopColor = borderColor;
            card.style.borderBottomColor = borderColor;
            card.style.borderLeftColor = borderColor;
            card.style.borderRightColor = borderColor;

            return card;
        }

        private static VisualElement CreateRunningHeader(string interfaceName, string className,
            Color pillColor, string pillLabel)
        {
            VisualElement header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = Space1;

            VisualElement titleBlock = new VisualElement();
            titleBlock.style.flexGrow = 1;
            titleBlock.Add(CreatePrefixedLine("Service Interface", interfaceName, true));
            titleBlock.Add(CreatePrefixedLine("Class Implementation", className, false));
            header.Add(titleBlock);

            VisualElement initBlock = new VisualElement();
            initBlock.style.alignItems = Align.FlexEnd;
            initBlock.style.flexShrink = 0;

            Label initHeader = new Label("Initialization Type");
            initHeader.style.color = MutedTextColor;
            initHeader.style.fontSize = 11;
            initBlock.Add(initHeader);

            initBlock.Add(CreateStatusPill(pillColor, pillLabel));
            header.Add(initBlock);

            return header;
        }

        private static VisualElement CreateTypeCardHeader(string prefix, string name, string subtitle)
        {
            VisualElement header = new VisualElement();
            header.style.marginBottom = Space1;
            header.Add(CreatePrefixedLine(prefix, name, true));

            if (!string.IsNullOrEmpty(subtitle))
            {
                Label subtitleLabel = new Label(subtitle);
                subtitleLabel.style.color = MutedTextColor;
                subtitleLabel.style.fontSize = 12;
                header.Add(subtitleLabel);
            }

            return header;
        }

        private static VisualElement CreatePrefixedLine(string prefix, string value, bool emphasize)
        {
            VisualElement line = new VisualElement();
            line.style.flexDirection = FlexDirection.Row;
            line.style.alignItems = Align.Center;

            Label prefixLabel = new Label($"{prefix}:");
            prefixLabel.style.color = MutedTextColor;
            prefixLabel.style.fontSize = 11;
            prefixLabel.style.marginRight = Space1;
            line.Add(prefixLabel);

            Label valueLabel = new Label(value);
            valueLabel.selection.isSelectable = true;
            valueLabel.style.fontSize = emphasize ? 14 : 13;

            if (emphasize)
            {
                valueLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            }

            line.Add(valueLabel);

            return line;
        }

        private static VisualElement CreateStatusPill(Color color, string text)
        {
            VisualElement pill = new VisualElement();
            pill.style.flexDirection = FlexDirection.Row;
            pill.style.alignItems = Align.Center;
            pill.style.flexShrink = 0;

            VisualElement dot = new VisualElement();
            dot.style.width = 8;
            dot.style.height = 8;
            dot.style.marginRight = Space1;
            dot.style.backgroundColor = color;
            SetBorderRadius(dot, 4);
            pill.Add(dot);

            Label label = new Label(text);
            label.style.color = color;
            label.style.fontSize = 12;
            pill.Add(label);

            return pill;
        }

        private static VisualElement CreateSubSection(string title)
        {
            VisualElement section = new VisualElement();
            section.style.marginTop = Space2;

            VisualElement separator = new VisualElement();
            separator.style.height = 1;
            separator.style.backgroundColor = SeparatorColor;
            separator.style.marginBottom = Space1;
            section.Add(separator);

            Label header = new Label(title.ToUpperInvariant());
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.fontSize = 11;
            header.style.color = MutedTextColor;
            header.style.marginBottom = Space1;
            section.Add(header);

            return section;
        }

        private static Label CreateInfoLabel(string text)
        {
            Label label = new Label(text);
            label.style.whiteSpace = WhiteSpace.Normal;

            return label;
        }

        private static void SetBorderRadius(VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }

        private static Color Saturated(Color color)
        {
            Color.RGBToHSV(color, out float hue, out float saturation, out float value);
            saturation = Mathf.Clamp01(saturation * 1.35f + 0.05f);
            value = Mathf.Clamp01(value + 0.15f);

            return Color.HSVToRGB(hue, saturation, value);
        }

        private readonly struct Card
        {
            public readonly VisualElement Root;
            public readonly Action Update;

            public Card(VisualElement root, Action update)
            {
                Root = root;
                Update = update;
            }
        }

        private readonly struct BadgeStyle
        {
            public readonly Type AttributeType;
            public readonly string Label;
            public readonly Color Color;
            public readonly string Tooltip;

            public BadgeStyle(Type attributeType, string label, Color color, string tooltip)
            {
                AttributeType = attributeType;
                Label = label;
                Color = color;
                Tooltip = tooltip;
            }
        }

        private sealed class AssemblyTabSection
        {
            private readonly VisualElement _host;
            private readonly Dictionary<string, ScrollView> _contentByAssembly = new();
            private readonly List<Action> _updaters = new();

            private TabView _tabView;
            private List<string> _assemblies = new();
            private string _selectedAssembly;
            private string _signature;
            private string _message;

            public AssemblyTabSection(VisualElement host)
            {
                _host = host;
            }

            public void Refresh<TItem>(IReadOnlyList<TItem> items, Func<TItem, string> assemblyOf,
                Func<TItem, string> signatureOf, Func<TItem, Card> cardFactory)
            {
                _message = null;

                string signature = BuildSignature(items, signatureOf);

                if (signature == _signature)
                {
                    foreach (Action update in _updaters)
                    {
                        update();
                    }

                    return;
                }

                _signature = signature;
                CaptureSelection();

                Dictionary<string, List<TItem>> grouped = new();

                foreach (TItem item in items)
                {
                    string assembly = assemblyOf(item);

                    if (!grouped.TryGetValue(assembly, out List<TItem> bucket))
                    {
                        bucket = new List<TItem>();
                        grouped[assembly] = bucket;
                    }

                    bucket.Add(item);
                }

                List<string> assemblies = grouped.Keys.OrderBy(name => name).ToList();

                if (!assemblies.SequenceEqual(_assemblies))
                {
                    RebuildTabs(assemblies);
                }

                _updaters.Clear();

                foreach (string assembly in assemblies)
                {
                    ScrollView content = _contentByAssembly[assembly];
                    Vector2 scrollOffset = content.scrollOffset;
                    content.Clear();

                    foreach (TItem item in grouped[assembly])
                    {
                        Card card = cardFactory(item);
                        content.Add(card.Root);

                        if (card.Update != null)
                        {
                            _updaters.Add(card.Update);
                        }
                    }

                    content.scrollOffset = scrollOffset;
                }
            }

            public void ShowMessage(string message)
            {
                if (_message == message)
                {
                    return;
                }

                _message = message;
                _signature = null;
                _assemblies = new List<string>();
                _contentByAssembly.Clear();
                _updaters.Clear();
                _tabView = null;
                _host.Clear();

                Label label = new Label(message);
                label.style.flexGrow = 1;
                label.style.color = MutedTextColor;
                label.style.unityTextAlign = TextAnchor.MiddleCenter;
                label.style.whiteSpace = WhiteSpace.Normal;
                _host.Add(label);
            }

            private static string BuildSignature<TItem>(IReadOnlyList<TItem> items, Func<TItem, string> signatureOf)
            {
                StringBuilder builder = new();

                foreach (TItem item in items)
                {
                    builder.Append(signatureOf(item)).Append('\n');
                }

                return builder.ToString();
            }

            private void CaptureSelection()
            {
                if (_tabView == null || _assemblies.Count == 0)
                {
                    return;
                }

                int index = _tabView.selectedTabIndex;

                if (index >= 0 && index < _assemblies.Count)
                {
                    _selectedAssembly = _assemblies[index];
                }
            }

            private void RebuildTabs(List<string> assemblies)
            {
                _assemblies = assemblies;
                _contentByAssembly.Clear();
                _host.Clear();

                _tabView = new TabView();
                _tabView.style.flexGrow = 1;

                foreach (string assembly in assemblies)
                {
                    Tab tab = new Tab(assembly);
                    ScrollView content = new ScrollView();
                    content.style.flexGrow = 1;
                    tab.Add(content);
                    _tabView.Add(tab);
                    _contentByAssembly[assembly] = content;
                }

                _host.Add(_tabView);

                int selectedIndex = _selectedAssembly != null ? assemblies.IndexOf(_selectedAssembly) : -1;

                if (selectedIndex < 0)
                {
                    selectedIndex = 0;
                }

                _tabView.selectedTabIndex = selectedIndex;
                _selectedAssembly = assemblies[selectedIndex];
            }
        }
    }
}
