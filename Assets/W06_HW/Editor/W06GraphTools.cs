#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace W06Homework
{
    /// <summary>Edits copied course graphs through the Unity 17.3 editor graph models.</summary>
    public static class W06GraphTools
    {
        const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static string InspectVfx(string path)
        {
            object resource;
            var graph = LoadVfx(path, out resource);
            var text = new StringBuilder(path).AppendLine();
            foreach (var model in Walk(graph))
            {
                text.Append(model.GetType().Name).Append(" [").Append(Get(model, "name")).Append(']');
                var attribute = GetOrNull(model, "attribute");
                if (attribute != null) text.Append(" attribute=").Append(attribute);
                text.AppendLine();
                foreach (var slot in Inputs(model))
                {
                    text.Append("  ").Append(Get(slot, "name")).Append(" = ").Append(Format(Get(slot, "value")));
                    text.Append("; links=").Append(Call(slot, "GetNbLinks")).AppendLine();
                }
            }
            return text.ToString();
        }

        public static void TuneShader(string path, float noiseScale, float clipStart)
        {
            RequireHomeworkAsset(path, ".shadergraph");
            var graphType = FindType("UnityEditor.ShaderGraph.GraphData");
            var graph = Activator.CreateInstance(graphType, true);
            Set(graph, "assetGuid", AssetDatabase.AssetPathToGUID(path));
            var messageProperty = Property(graphType, "messageManager");
            if (messageProperty != null)
                messageProperty.SetValue(graph, Activator.CreateInstance(messageProperty.PropertyType, true));

            var multiJson = FindType("UnityEditor.ShaderGraph.Serialization.MultiJson");
            var deserialize = multiJson.GetMethods(StaticFlags).Single(m => m.Name == "Deserialize" && m.IsGenericMethodDefinition);
            deserialize.MakeGenericMethod(graphType).Invoke(null, new object[] { graph, File.ReadAllText(path), null, false });
            Call(graph, "OnEnable");
            Call(graph, "ValidateGraph");

            var nodeType = FindType("UnityEditor.ShaderGraph.AbstractMaterialNode");
            var nodes = Enumerate(graphType.GetMethods(InstanceFlags).Single(m => m.Name == "GetNodes" && m.IsGenericMethodDefinition)
                .MakeGenericMethod(nodeType).Invoke(graph, null)).ToArray();
            var noise = ExactlyOne(nodes.Where(n => n.GetType().Name == "NoiseNode"), "Simple Noise node");
            Set(ShaderSlot(noise, 1), "value", noiseScale);

            var clipBlock = ExactlyOne(nodes.Where(n => Convert.ToString(Get(n, "name"), CultureInfo.InvariantCulture)
                .EndsWith("AlphaClipThreshold", StringComparison.Ordinal)), "Alpha Clip Threshold block");
            var clipEdge = ExactlyOne(Enumerate(Get(graph, "edges")).Where(e => ReferenceEquals(Get(Get(e, "inputSlot"), "node"), clipBlock)),
                "edge entering Alpha Clip Threshold");
            var clipLerp = Get(Get(clipEdge, "outputSlot"), "node");
            if (clipLerp.GetType().Name != "LerpNode")
                throw new InvalidOperationException("Expected a Lerp node driving Alpha Clip Threshold, got " + clipLerp.GetType().FullName);
            Set(ShaderSlot(clipLerp, 0), "value", new Vector4(clipStart, 0f, 0f, 0f));
            Call(graph, "ValidateGraph");

            var serialize = multiJson.GetMethods(StaticFlags).Single(m => m.Name == "Serialize" && m.GetParameters().Length == 1);
            var serialized = (string)serialize.Invoke(null, new[] { graph });
            var graphUtil = FindType("UnityEditor.ShaderGraph.GraphUtil");
            var write = graphUtil.GetMethod("WriteToFile", StaticFlags, null, new[] { typeof(string), typeof(string) }, null);
            if (write == null || !(bool)write.Invoke(null, new object[] { path, serialized }))
                throw new IOException("Shader Graph's native WriteToFile failed for " + path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log("[W06] Shader tuned: " + path + "; Noise Scale=" + noiseScale + "; clip start=" + clipStart);
        }

        public static void TuneVfx(string path, string shaderPath, bool concrete)
        {
            RequireHomeworkAsset(path, ".vfx");
            RequireHomeworkAsset(shaderPath, ".shadergraph");
            object resource;
            var graph = LoadVfx(path, out resource);
            var models = Walk(graph).ToArray();
            var spawner = Model(models, "VFXBasicSpawner");
            SetSetting(spawner, "loopDuration", "Constant");
            SetSetting(spawner, "loopCount", "Constant");
            SetInput(spawner, "LoopDuration", 1f);
            SetInput(spawner, "LoopCount", 1);
            SetInput(Model(models, "VFXSpawnerBurst"), "Count", concrete ? 16f : 27f);

            var initialize = Model(models, "VFXBasicInitialize");
            var data = Call(initialize, "GetData");
            SetSetting(data, "capacity", concrete ? 64u : 96u);
            SetSetting(data, "boundsMode", "Manual");
            SetInput(initialize, "bounds", new Bounds(Vector3.zero, new Vector3(7f, 7f, 7f)));

            var lifetime = Attribute(models, "lifetime");
            SetInput(lifetime, "A", concrete ? .55f : .9f);
            SetInput(lifetime, "B", concrete ? .85f : 1.45f);
            SetInput(Attribute(models, "size"), "_Size", concrete ? .105f : .065f);
            SetInput(Attribute(models, "scale"), "_Scale", concrete ? new Vector3(1f, .8f, 1f) : new Vector3(.55f, 1.7f, 1f));
            EnsureRandomInitialAngle(initialize);

            var velocity = Model(models, "VelocityDirection");
            SetInput(velocity, "Direction", Vector3.up);
            SetInput(velocity, "MinSpeed", concrete ? 1.5f : 2.8f);
            SetInput(velocity, "MaxSpeed", concrete ? 3.8f : 5.8f);
            SetInput(Model(models, "Gravity"), "Force", new Vector3(0f, concrete ? -13f : -10.8f, 0f));
            SetInput(Model(models, "Drag"), "dragCoefficient", concrete ? 1.15f : .65f);

            var output = Model(models, "VFXComposedParticleOutput");
            var shader = AssetDatabase.LoadAllAssetsAtPath(shaderPath).SingleOrDefault(a => a != null && a.GetType().Name == "ShaderGraphVfxAsset");
            if (shader == null) throw new InvalidOperationException("No ShaderGraphVfxAsset imported at " + shaderPath + ". Check Support VFX Graph and shader compilation.");
            SetSetting(output, "shaderGraph", shader);
            var chip = concrete ? new Color(.36f, .39f, .41f, 1f) : new Color(.30f, .105f, .026f, 1f);
            // The concrete edge stays in the ordinary color range; only wood has an HDR burning edge.
            var edge = concrete ? chip : new Color(1f, .285f, .025f, 1f) * 2.4f;
            edge.a = 1f;
            SetInput(output, "_Color_Chip", chip);
            SetInput(output, "_Color_Edge", edge);
            SetInput(output, "_Edge_Width", concrete ? .015f : .035f);

            var dissolve = Input(output, "_Dissolve_Amount");
            if (concrete)
            {
                Call(dissolve, "UnlinkAll", false, true);
                SetSlot(dissolve, 0f);
            }
            else
            {
                if (Convert.ToInt32(Call(dissolve, "GetNbLinks"), CultureInfo.InvariantCulture) < 1)
                    throw new InvalidOperationException("Wood Dissolve_Amount lost its Age Over Lifetime connection. Refusing to save an incomplete effect.");
                Debug.Log("[W06] Retained wood Age Over Lifetime -> Dissolve_Amount connection.");
            }

            foreach (var model in models.OfType<UnityEngine.Object>()) EditorUtility.SetDirty(model);
            EditorUtility.SetDirty((UnityEngine.Object)resource);
            var extensions = FindType("UnityEditor.VFX.VisualEffectResourceExtensions");
            extensions.GetMethod("WriteAssetWithSubAssets", StaticFlags).Invoke(null, new[] { resource });
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log("[W06] VFX tuned and saved: " + path + "; material=" + (concrete ? "concrete" : "wood") + "\n" + InspectVfx(path));
        }

        static void EnsureRandomInitialAngle(object initialize)
        {
            var matches = Enumerate(Get(initialize, "children")).Where(model =>
            {
                if (model.GetType().Name != "SetAttribute") return false;
                var attribute = Convert.ToString(Get(model, "attribute"), CultureInfo.InvariantCulture);
                var channels = Convert.ToString(Get(model, "channels"), CultureInfo.InvariantCulture);
                return attribute == "angleZ" || attribute == "angle" && channels == "Z";
            }).ToArray();
            if (matches.Length > 1)
                throw new InvalidOperationException("Initialize contains multiple angle.Z blocks; refusing to stack conflicting initial rotations.");

            var added = matches.Length == 0;
            var angle = added ? ScriptableObject.CreateInstance(FindType("UnityEditor.VFX.Block.SetAttribute")) : matches[0];
            // Unity 17.3 stores the canonical variadic attribute as 'angle' with channel Z.
            // The legacy 'angleZ' spelling is converted to this representation on import.
            SetSetting(angle, "attribute", "angle");
            SetSetting(angle, "channels", "Z");
            SetSetting(angle, "Source", "Slot");
            SetSetting(angle, "Composition", "Overwrite");
            SetSetting(angle, "Random", "Uniform");
            if (added) Call(initialize, "AddChild", angle, -1, true);
            SetInput(angle, "A", 0f);
            SetInput(angle, "B", 360f);
            if (Convert.ToString(Get(angle, "attribute"), CultureInfo.InvariantCulture) != "angle"
                || Convert.ToString(Get(angle, "channels"), CultureInfo.InvariantCulture) != "Z"
                || Convert.ToString(Get(angle, "Random"), CultureInfo.InvariantCulture) != "Uniform")
                throw new InvalidOperationException("Native graph did not retain the random angle.Z settings.");
            EditorUtility.SetDirty((UnityEngine.Object)angle);
            Debug.Log("[W06] " + (added ? "Added" : "Reused") + " Initialize angle.Z: random uniform 0..360 degrees (slot values verified).");
        }

        static object ShaderSlot(object node, int id)
        {
            var slotType = FindType("UnityEditor.ShaderGraph.MaterialSlot");
            var method = node.GetType().GetMethods(InstanceFlags).Single(m => m.Name == "FindInputSlot" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1);
            var result = method.MakeGenericMethod(slotType).Invoke(node, new object[] { id });
            if (result == null) throw new InvalidOperationException(node.GetType().Name + " has no input slot " + id);
            return result;
        }

        static object LoadVfx(string path, out object resource)
        {
            var resourceType = FindType("UnityEditor.VFX.VisualEffectResource");
            var getResource = resourceType.GetMethod("GetResourceAtPath", StaticFlags, null, new[] { typeof(string) }, null);
            if (getResource == null) throw new MissingMethodException(resourceType.FullName, "GetResourceAtPath(string)");
            resource = getResource.Invoke(null, new object[] { path });
            if (resource == null) throw new InvalidOperationException("VFX resource not imported at " + path);
            var extensions = FindType("UnityEditor.VFX.VisualEffectResourceExtensions");
            return extensions.GetMethod("GetOrCreateGraph", StaticFlags).Invoke(null, new[] { resource });
        }

        static IEnumerable<object> Walk(object model)
        {
            yield return model;
            foreach (var child in Enumerate(GetOrNull(model, "children")).ToArray())
                foreach (var descendant in Walk(child)) yield return descendant;
        }

        static object Model(IEnumerable<object> models, string typeName)
        {
            return ExactlyOne(models.Where(m => m.GetType().Name == typeName), typeName);
        }

        static object Attribute(IEnumerable<object> models, string attribute)
        {
            return ExactlyOne(models.Where(m => m.GetType().Name == "SetAttribute" && Convert.ToString(Get(m, "attribute"), CultureInfo.InvariantCulture) == attribute),
                "SetAttribute(" + attribute + ")");
        }

        static object ExactlyOne(IEnumerable<object> values, string label)
        {
            var array = values.ToArray();
            if (array.Length != 1) throw new InvalidOperationException("Expected exactly one " + label + ", found " + array.Length);
            return array[0];
        }

        static IEnumerable<object> Inputs(object model)
        {
            return Enumerate(GetOrNull(model, "inputSlots")).Where(s => s != null);
        }

        static object Input(object model, string name)
        {
            var slots = Inputs(model).ToArray();
            var slot = slots.SingleOrDefault(s => Convert.ToString(Get(s, "name"), CultureInfo.InvariantCulture) == name);
            if (slot == null)
                throw new InvalidOperationException(model.GetType().Name + " has no input '" + name + "'. Available: " + string.Join(", ", slots.Select(s => Get(s, "name"))));
            return slot;
        }

        static void SetInput(object model, string name, object value) { SetSlot(Input(model, name), value); }

        static void SetSlot(object slot, object value)
        {
            var property = Get(slot, "property");
            var valueType = (Type)Get(property, "type");
            var converted = ConvertValue(value, valueType);
            Set(slot, "value", converted);
            var actual = Get(slot, "value");
            if (!Equals(actual, converted))
                throw new InvalidOperationException("VFX slot rejected or changed value: " + Get(slot, "name")
                    + "; requested=" + Format(converted) + "; actual=" + Format(actual));
            Debug.Log("[W06] " + Get(slot, "name") + " = " + Format(actual));
        }

        static void SetSetting(object model, string name, object value)
        {
            var setting = Call(model, "GetSetting", name);
            var field = GetOrNull(setting, "field") as FieldInfo;
            if (field == null) throw new InvalidOperationException(model.GetType().Name + " has no graph setting '" + name + "'.");
            Call(model, "SetSettingValue", name, ConvertValue(value, field.FieldType));
        }

        static object ConvertValue(object value, Type target)
        {
            if (value == null || target.IsInstanceOfType(value)) return value;
            if (target.IsEnum) return value is string ? Enum.Parse(target, (string)value) : Enum.ToObject(target, value);
            if (value is Vector3)
            {
                var vectorField = target.GetFields(InstanceFlags).SingleOrDefault(f => f.FieldType == typeof(Vector3));
                if (vectorField != null)
                {
                    var boxed = Activator.CreateInstance(target);
                    vectorField.SetValue(boxed, value);
                    return boxed;
                }
            }
            if (value is Bounds)
            {
                var bounds = (Bounds)value;
                var boxed = Activator.CreateInstance(target);
                var center = target.GetField("center", InstanceFlags);
                var size = target.GetField("size", InstanceFlags);
                if (center != null && size != null)
                {
                    center.SetValue(boxed, bounds.center);
                    size.SetValue(boxed, bounds.size);
                    return boxed;
                }
            }
            return Convert.ChangeType(value, target, CultureInfo.InvariantCulture);
        }

        static object Call(object target, string name, params object[] arguments)
        {
            for (var type = target.GetType(); type != null; type = type.BaseType)
            {
                var methods = type.GetMethods(InstanceFlags | BindingFlags.DeclaredOnly).Where(m => m.Name == name && !m.IsGenericMethodDefinition && m.GetParameters().Length == arguments.Length).ToArray();
                if (methods.Length == 1) return methods[0].Invoke(target, arguments);
                if (methods.Length > 1)
                {
                    var compatible = methods.Where(m => m.GetParameters().Select((p, i) => arguments[i] == null || p.ParameterType.IsInstanceOfType(arguments[i])).All(x => x)).ToArray();
                    if (compatible.Length == 1) return compatible[0].Invoke(target, arguments);
                    throw new AmbiguousMatchException(type.FullName + "." + name);
                }
            }
            throw new MissingMethodException(target.GetType().FullName, name + "(" + arguments.Length + " args)");
        }

        static object Get(object target, string name)
        {
            var property = Property(target.GetType(), name);
            if (property != null) return property.GetValue(target);
            var field = Field(target.GetType(), name);
            if (field != null) return field.GetValue(target);
            throw new MissingMemberException(target.GetType().FullName, name);
        }

        static object GetOrNull(object target, string name)
        {
            if (target == null) return null;
            var property = Property(target.GetType(), name);
            if (property != null) return property.GetValue(target);
            var field = Field(target.GetType(), name);
            return field == null ? null : field.GetValue(target);
        }

        static void Set(object target, string name, object value)
        {
            var property = Property(target.GetType(), name);
            if (property != null) { property.SetValue(target, value); return; }
            var field = Field(target.GetType(), name);
            if (field != null) { field.SetValue(target, value); return; }
            throw new MissingMemberException(target.GetType().FullName, name);
        }

        static PropertyInfo Property(Type type, string name)
        {
            for (; type != null; type = type.BaseType)
            {
                var property = type.GetProperty(name, InstanceFlags | BindingFlags.DeclaredOnly);
                if (property != null) return property;
            }
            return null;
        }

        static FieldInfo Field(Type type, string name)
        {
            for (; type != null; type = type.BaseType)
            {
                var field = type.GetField(name, InstanceFlags | BindingFlags.DeclaredOnly);
                if (field != null) return field;
            }
            return null;
        }

        static Type FindType(string fullName)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(fullName, false)).FirstOrDefault(t => t != null);
            if (type == null) throw new TypeLoadException("Unity editor graph type is not loaded: " + fullName);
            return type;
        }

        static IEnumerable<object> Enumerate(object values)
        {
            var enumerable = values as IEnumerable;
            if (enumerable == null) yield break;
            foreach (var value in enumerable) if (value != null) yield return value;
        }

        static string Format(object value)
        {
            if (value == null) return "null";
            if (value is float) return ((float)value).ToString("G6", CultureInfo.InvariantCulture);
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        static void RequireHomeworkAsset(string path, string extension)
        {
            var normalized = path.Replace('\\', '/');
            if (!normalized.StartsWith("Assets/W06_HW/", StringComparison.Ordinal) || !normalized.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Only copied homework assets under Assets/W06_HW may be edited: " + path);
            if (!File.Exists(path)) throw new FileNotFoundException("Homework asset does not exist.", path);
        }
    }
}
#endif
