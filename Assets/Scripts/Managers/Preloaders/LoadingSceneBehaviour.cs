using System;

[Serializable]
public abstract class Preloader<T> : IPreloader where T: struct
{
    public abstract IPreloader CommencerChargement(Action<T> onComplete);
}

public interface IPreloader
{
}