using System.Diagnostics;

namespace TableRW.Read.I;

public enum IterAction {
    //None,
    /// <summary> End the read of the row and add it to the collection </summary>
    EndRow = 1,
    /// <summary> Skip the row and do not add it to the collection </summary>
    SkipRow,
    /// <summary> End the table read, this row will not be added to the collection </summary>
    EndTable,
}

public interface ISource<out TSource> {
    /// <summary> Index column </summary>
    public int iCol { get; internal set; }

    /// <summary> Index row </summary>
    public int iRow { get; internal set; }

    /// <summary> Data source </summary>
    public TSource Src { get; }
}

public interface IEntity<TEntity> {
    /// <summary> Current entities </summary>
    public TEntity Entity { get; internal set; }

    /// <summary> Previous entity </summary>
    public TEntity PreEntity { get; internal set; }
}

//public interface IData<D> {
//    public D Data { get; }
//}

public interface IContext<out TSource, TEntity> : IEntity<TEntity>, ISource<TSource> {

    /// <summary> End the read of the row and add it to the collection </summary>
    public IterAction? EndRow(bool condition = true);

    /// <summary> Skip the row and do not add it to the collection </summary>
    public IterAction? SkipRow(bool condition = true);

    /// <summary> End the table read, this row will not be added to the collection </summary>
    public IterAction? EndTable(bool condition = true);
}

public interface IContext<out TSource, TEntity, D> : IContext<TSource, TEntity> {
    public D Data { get; set; }
}

public interface ISubContext<out TSource, TEntity, P> : IContext<TSource, TEntity> {
    public P Parent { get; internal set; }
}

public interface ISubContext<out TSource, TEntity, P, D>
: ISubContext<TSource, TEntity, P> {
    public D Data { get; set; }
}

public struct ParentEntity<T> {
    public ParentEntity(T entity) => Entity = entity;
    public T Entity { get; }
}

public struct ParentEntity<T, P> {
    public ParentEntity(T entity, P parent) => (Entity, Parent) = (entity, parent);
    public T Entity { get; }
    public P Parent { get; }
}

[DebuggerDisplay("iRow = {iRow}, iCol = {iCol}, Entity = \\{ {Entity} \\}")]
internal class Context<TSource, TEntity, D>(TSource src)
: IContext<TSource, TEntity, D> {

    public D Data { get; set; } = default!;

    public TSource Src { get; set; } = src;

    public TEntity Entity { get; set; } = default!;

    public TEntity PreEntity { get; set; } = default!;

    public int iCol { get; set; } = 0;

    public int iRow { get; set; } = 0;

    public IterAction? EndRow(bool condition = true)
        => condition ? IterAction.EndRow : null;

    public IterAction? SkipRow(bool condition = true)
        => condition ? IterAction.SkipRow : null;

    public IterAction? EndTable(bool condition = true)
        => condition ? IterAction.EndTable : null;
}

[DebuggerDisplay("iRow = {iRow}, iCol = {iCol}, Entity = \\{ {Entity} \\}")]
internal class SubContext<TSource, TEntity, P, D>(TSource src)
: Context<TSource, TEntity, D>(src)
, ISubContext<TSource, TEntity, P, D> {

    public P Parent { get; set; } = default!;
}