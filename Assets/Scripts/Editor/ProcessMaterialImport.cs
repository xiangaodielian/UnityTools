/********************************************************************
* Author:	WuWenhua
* Create:	2022/3/22
* Note  :	模型导入时不导入默认材质
*********************************************************************/
using UnityEngine;
using UnityEditor;

public class ProcessMaterialImport : AssetPostprocessor
{
    void OnPostprocessModel(GameObject model)
    {
        var renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers == null)
        {
            return;
        }
        foreach (var renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }
            renderer.sharedMaterials = new Material[0];
        }

        ModelImporter importer = (ModelImporter)base.assetImporter;
        if (importer != null)
        {
            if (importer.materialImportMode != ModelImporterMaterialImportMode.None)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.SaveAndReimport();
            }
        }
    }
}
