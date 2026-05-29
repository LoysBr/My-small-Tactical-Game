using UnityEngine;

public class EnemyModel
{
    public Vector3 Position { get; private set; }

    public EnemyPresenter m_Presenter { get; private set; }

    public EnemyModel(EnemyPresenter presenter)
    {  
        m_Presenter = presenter;
        Position = presenter.transform.position;
    }
}
