using UnityEngine;

public class ControllerAppletCheck : Behavior
{
    public GameObject m_oUI;

#if UNITY_SWITCH
    public override void activate()
    {
        base.activate();

        //m_oUI.SetActive(true);
    }

    public override void update()
    {
        base.update();

        if (RewiredManager.Instance.GetMenuPlayerActionDown("UIRandom"))
        {
            SwitchManager.Instance.showControllerApplet();
        }
    }

    public override void deactivate()
    {
        base.deactivate();

        //m_oUI.SetActive(false);
    }
#else
        public override void activate()
        {
            base.activate();
    
            m_oUI.SetActive(false);
        }
#endif
}
