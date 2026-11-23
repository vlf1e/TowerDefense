using UnityEngine;
using UnityEngine.EventSystems;
using TowerDefense.Data;
using TowerDefense.Towers;

namespace TowerDefense.UI
{
    public class TowerSlotUI : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private TowerType towerType;

        public void Initialize(TowerType type)
        {
            towerType = type;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (TowerPlacementController.Instance != null)
                {
                    TowerPlacementController.Instance.StartPlacement(towerType);
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (TowerPlacementController.Instance != null && !TowerPlacementController.Instance.IsPlacing)
                {
                    TowerPlacementController.Instance.StartPlacement(towerType);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (TowerPlacementController.Instance != null && !TowerPlacementController.Instance.IsPlacing)
                {
                    TowerPlacementController.Instance.StartPlacement(towerType);
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Позиция превью автоматически обновляется в TowerPlacementController.Update()
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (TowerPlacementController.Instance != null && TowerPlacementController.Instance.IsPlacing)
            {
                TowerPlacementController.Instance.TryPlaceAtMouse();
            }
        }
    }
}
