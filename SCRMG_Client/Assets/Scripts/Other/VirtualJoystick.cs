using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler {

    //References
    Toolbox toolbox;
    EventManager em;
    Image joystickBackground;
    Image joystickImage;
    //Variables
    int index = -1;

    private Vector2 inputDirection = Vector3.zero;

    private void Awake()
    {
        toolbox = FindObjectOfType<Toolbox>();
        em = toolbox.GetComponent<EventManager>();
        joystickBackground = GetComponent<Image>();
        joystickImage = transform.GetChild(0).GetComponent<Image>();
    }
    
    public void SetIndex(int newIndex)
    {
        index = newIndex;
    }

    public virtual void OnDrag(PointerEventData ped)
    {
        Vector2 position = Vector2.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground.rectTransform, ped.position, ped.pressEventCamera,
            out position))
        {
            position.x = (position.x / joystickBackground.rectTransform.sizeDelta.x);
            position.y = (position.y / joystickBackground.rectTransform.sizeDelta.y);
            
            float x = position.x * 2;
            float y = position.y * 2;

            inputDirection = new Vector3(x, y);
            if (inputDirection.sqrMagnitude > 1)
            {
                inputDirection.Normalize();
            }
            
            em.BroadcastVirtualJoystickValueChange(index, inputDirection);

            joystickImage.rectTransform.anchoredPosition =
                new Vector3(inputDirection.x * (joystickBackground.rectTransform.sizeDelta.x / 10),
                inputDirection.y * (joystickBackground.rectTransform.sizeDelta.y / 10));
        }
    }

    public virtual void OnPointerDown(PointerEventData ped)
    {
        em.BroadcastVirtualJoystickPressed(index);
        OnDrag(ped);
    }

    public virtual void OnPointerUp(PointerEventData ped)
    {
        em.BroadcastVirtualJoystickReleased(index);
        inputDirection = Vector3.zero;
        em.BroadcastVirtualJoystickValueChange(index, inputDirection);
        joystickImage.rectTransform.anchoredPosition = Vector3.zero;
    }

}
