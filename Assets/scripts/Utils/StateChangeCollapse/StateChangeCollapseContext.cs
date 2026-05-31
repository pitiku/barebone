using System;

namespace StateChangeCollapse
{
    //Overarching class to unify all state collapse contexts
    public class StateChangeCollapseContext : SceneSingleton<StateChangeCollapseContext>
    {
        [NonSerialized] public readonly GameObjectStateContext m_oGameObjectStateContext = new();
        [NonSerialized] public readonly TextStateContext m_oTextStateContext = new();
        [NonSerialized] public readonly SliderStateContext m_oSliderStateContext = new();
        [NonSerialized] public readonly LayerStateContext m_oLayerStateContext = new();
        [NonSerialized] public readonly AnimatorStateContext m_oAnimatorStateContext = new();

        public void start()
        {
            m_oGameObjectStateContext.start();
            m_oTextStateContext.start();
            m_oSliderStateContext.start();
            m_oLayerStateContext.start();
            m_oAnimatorStateContext.start();
        }

        public void end()
        {
            m_oGameObjectStateContext.end();
            m_oTextStateContext.end();
            m_oSliderStateContext.end();
            m_oLayerStateContext.end();
            m_oAnimatorStateContext.end();
        }

        //Scopes start/end in a using statement
        public AutoScope auto() => new AutoScope(this);
        public readonly ref struct AutoScope
        {
            private readonly StateChangeCollapseContext m_oInstance;

            public AutoScope(StateChangeCollapseContext _oInstance)
            {
                m_oInstance = _oInstance;
                m_oInstance.start();
            }

            public void Dispose()
            {
                m_oInstance.end();
            }
        }
    }
}