using UnityEngine;

public class EnemyModel
{
    public Vector3 Position { get; private set; }

    public EnemyPresenter Presenter { get; private set; }

    public EnemyModel(EnemyPresenter presenter)
    {
        Presenter = presenter;
        Position = presenter.transform.position;
    }
}
