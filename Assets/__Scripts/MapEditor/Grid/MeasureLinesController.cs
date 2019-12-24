﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MeasureLinesController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI measureLinePrefab;
    [SerializeField] private AudioTimeSyncController atsc;
    [SerializeField] private RectTransform parent;
    [SerializeField] private Transform noteGrid;
    [SerializeField] private Transform frontNoteGridScaling;
    [SerializeField] private Transform measureLineGrid;
    [SerializeField] private UIWorkflowToggle workflowToggle;

    private float previousATSCBeat = -1;
    private Dictionary<int, TextMeshProUGUI> measureTextsByBeat = new Dictionary<int, TextMeshProUGUI>();
    private Dictionary<int, bool> previousEnabledByBeat = new Dictionary<int, bool>();

    private bool init = false;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f); //wait for ATSC to get song info, so we can get beats in the song VVV
        measureTextsByBeat.Add(0, measureLinePrefab);
        previousEnabledByBeat.Add(0, true);
        for (int i = 1; i <= Mathf.FloorToInt(atsc.GetBeatFromSeconds(BeatSaberSongContainer.Instance.loadedSong.length)); i++)
        {
            TextMeshProUGUI instantiate = Instantiate(measureLinePrefab, parent);
            instantiate.text = $"{i}";
            instantiate.transform.localPosition = new Vector3(0, i * EditorScaleController.EditorScale, 0);
            measureTextsByBeat.Add(i, instantiate);
            previousEnabledByBeat.Add(i, true);
        }
        init = true;
        UpdateParentPosition();
    }

    void Update()
    {
        if (atsc.CurrentBeat == previousATSCBeat || !init) return;
        previousATSCBeat = atsc.CurrentBeat;
        float offsetBeat = atsc.CurrentBeat - atsc.offsetBeat;
        float beatsAhead = frontNoteGridScaling.localScale.z / EditorScaleController.EditorScale;
        float beatsBehind = beatsAhead / 4f;
        foreach (KeyValuePair<int, TextMeshProUGUI> kvp in measureTextsByBeat)
        {
            bool enabled = kvp.Key >= offsetBeat - beatsBehind && kvp.Key <= offsetBeat + beatsAhead;
            kvp.Value.transform.localPosition = new Vector3(0, kvp.Key * EditorScaleController.EditorScale, 0);
            if (previousEnabledByBeat[kvp.Key] != enabled)
            {
                kvp.Value.gameObject.SetActive(enabled);
                previousEnabledByBeat[kvp.Key] = enabled;
            }
        }
    }

    public void UpdateParentPosition()
    {
        Transform gridParent = workflowToggle.SelectedWorkflowGroup == 0 ? noteGrid : measureLineGrid;
        parent.transform.SetParent(gridParent);
        float x = workflowToggle.SelectedWorkflowGroup == 0 ? 1.2f + ((noteGrid.localScale.x - 0.01f) * 5) : 14;
        parent.transform.localEulerAngles = Vector3.right * 90;
        parent.transform.localScale = new Vector3(1f / gridParent.localScale.x, 1f / gridParent.localScale.y,
            1f / gridParent.localScale.z);
        parent.transform.localPosition = new Vector3(x * (1f / gridParent.localScale.x), atsc.gridStartPosition, 0);
    }
}
