﻿using UnityEngine;

public class HexFeatureManager : MonoBehaviour
{
    public Transform[] featuresPrefab;

    Transform featuresContainer;

    public void Clear()
    {
        if(featuresContainer)
            Destroy(featuresContainer.gameObject);
        featuresContainer = new GameObject("Features Container").transform;
        featuresContainer.SetParent(transform, false);
    }

    public void Apply()
    {

    }

    public void AddFeature(HexCell cell)
    {
        Transform instance = Instantiate(featuresPrefab[cell.FeatureIndex - 1]);
        Vector3 position = cell.Position;
        position.y += instance.localScale.y * 0.5f;
        instance.localPosition = position;
        instance.SetParent(featuresContainer, false);
    }
}