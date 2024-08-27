using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieMenuUI : MonoBehaviour
{
    [SerializeField] RectTransform pointerUI;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        Vector2 direction = GameInput.I.GetPieDirectionVector();

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        pointerUI.localRotation = Quaternion.Euler(0, 0, angle);

    }
}
