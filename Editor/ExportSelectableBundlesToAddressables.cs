using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace PT.ContentManager
{
    public class ExportSelectableBundlesToAddressables : EditorWindow
    {
        #region Variables
        private Vector2 _scrollPos;
        private List<string> _bundleNames = new List<string>();
        private List<bool> _selected;
        private string _filter = "";
        private bool _selectAllFiltered = false;
        #endregion

        [MenuItem("Window/Asset Management/Tools/Export Selectable Bundles To Addressables")]
        public static void ShowWindow()
        {
            GetWindow<ExportSelectableBundlesToAddressables>("Export Bundles");
        }

        private void OnEnable()
        {
            Init();
        }

        #region Draw
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Select Bundles to Export", EditorStyles.boldLabel);

            DrawFilter();

            DrawSelectAllFilterToggle();

            DrawAllBundlesNames();

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Export Selected Bundles"))
            {
                ExportSelectedBundles();
            }
        }
        
        private void DrawFilter()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filter:", GUILayout.Width(40));
            _filter = EditorGUILayout.TextField(_filter);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSelectAllFilterToggle()
        {
            bool prevSelectAll = _selectAllFiltered;
            _selectAllFiltered = EditorGUILayout.ToggleLeft("Select All (Filtered)", _selectAllFiltered);

            if (_selectAllFiltered == prevSelectAll)
            {
                return;
            }
            
            for (int i = 0; i < _bundleNames.Count; i++)
            {
                if (string.IsNullOrEmpty(_filter) || _bundleNames[i].ToLower().Contains(_filter.ToLower()))
                {
                    _selected[i] = _selectAllFiltered;
                }
            }
        }
        
        private void DrawAllBundlesNames()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            for (int i = 0; i < _bundleNames.Count; i++)
            {
                if (!string.IsNullOrEmpty(_filter) && !_bundleNames[i].ToLower().Contains(_filter.ToLower()))
                {
                    continue;
                }

                _selected[i] = EditorGUILayout.ToggleLeft(_bundleNames[i], _selected[i]);
            }
        }
       
        
        #endregion
        
        #region Logic

        private void Init()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
            _bundleNames = new List<string>(AssetDatabase.GetAllAssetBundleNames());
            _selected = new List<bool>(new bool[_bundleNames.Count]);
        }

        private void ExportSelectedBundles()
        {
            var selectedBundles = new List<string>();
            for (int i = 0; i < _bundleNames.Count; i++)
            {
                if (_selected[i])
                {
                    selectedBundles.Add(_bundleNames[i]);
                }
            }

            if (selectedBundles.Count == 0)
            {
                EditorUtility.DisplayDialog("No Bundles Selected", "Please select at least one bundle.", "OK");
                return;
            }

            ConvertAssetBundlesToAddressables(selectedBundles);
        }

        //This code is from https://github.com/needle-mirror/com.unity.addressables/blob/7e648bde5ba068618aca75e03b38b176fb969c08/Editor/Settings/AddressableAssetUtility.cs#L205
        static void ConvertAssetBundlesToAddressables(List<string> bundleList)
        {
            if (bundleList == null || bundleList.Count == 0)
            {
                Debug.LogWarning("No asset bundles to convert.");
                return;
            }

            float fullCount = bundleList.Count;
            int currCount = 0;

            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            foreach (var bundle in bundleList)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Converting Legacy Asset Bundles", bundle, currCount / fullCount))
                {
                    break;
                }

                currCount++;
                var group = settings.CreateGroup(bundle, false, false, false, null);
                var schema = group.AddSchema<BundledAssetGroupSchema>();

                var method = typeof(BundledAssetGroupSchema).GetMethod("Validate", BindingFlags.Instance | BindingFlags.NonPublic);
                if (method != null)
                {
                    method.Invoke(schema, null);
                }

                schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
                group.AddSchema<ContentUpdateGroupSchema>().StaticContent = true;

                var assetList = AssetDatabase.GetAssetPathsFromAssetBundle(bundle);

                foreach (var asset in assetList)
                {
                    var guid = AssetDatabase.AssetPathToGUID(asset);
                    settings.CreateOrMoveEntry(guid, group, false, false);
                    var imp = AssetImporter.GetAtPath(asset);
                    if (imp != null)
                    {
                        imp.SetAssetBundleNameAndVariant(string.Empty, string.Empty);
                    }
                }
            }

            if (fullCount > 0)
            {
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.RemoveUnusedAssetBundleNames();
        }
        
        #endregion
    }
}