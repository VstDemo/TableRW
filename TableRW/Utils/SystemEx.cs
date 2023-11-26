namespace TableRW.Utils.Ex;

static class SystemEx {
#if NET7_0_OR_GREATER
    internal static T NonZeroOr<T>(this T x, T orValue) where T : INumber<T>
        => x == T.Zero ? orValue : x;
#else
    internal static int NonZeroOr(this int x, int orValue) => x == 0 ? orValue : x;
#endif
}

