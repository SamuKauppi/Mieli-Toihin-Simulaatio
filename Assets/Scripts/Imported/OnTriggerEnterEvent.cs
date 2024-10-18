using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// Kutsutaan kun joku objekti menee triggerin sisään
public class OnTriggerEnterEvent : MonoBehaviour
{
    public string RequiredTag;

    public UnityEvent TriggerEnterEvent;

    public bool DisableAfterTriggering;

	public float Delay;

    bool isDisabled;

    /// <summary>
    /// Kutsutaan kun joku objekti menee triggerin sisään
    /// </summary>
    /// <param name="other">Other.</param>
    protected IEnumerator OnTriggerEnter(Collider other)
    {
        if (!isDisabled)
        {
            if (Delay > 0)
            {
                yield return new WaitForSeconds(Delay);
            }

            if (string.IsNullOrEmpty(RequiredTag) || other.tag == RequiredTag)
            {
                if (TriggerEnterEvent != null)
                {
                    TriggerEnterEvent.Invoke();
                }

                if (DisableAfterTriggering)
                    isDisabled = true;
            }
        }
    }
}
