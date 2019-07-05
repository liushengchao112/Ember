

public delegate void Callback();
public delegate void Callback<T>(T arg1);
public delegate void Callback<T, U>(T arg1, U arg2);
public delegate void Callback<T, U, V>(T arg1, U arg2, V arg3);
public delegate void Callback<T, U, V, F>(T arg1, U arg2, V arg3, F arg4);
public delegate void Callback<T, U, V, F, K>(T arg1, U arg2, V arg3, F arg4, K arg5);
public delegate object CallbackReturn();
public delegate object CallbackReturn<T>(T arg1);
public delegate object CallbackReturn<T, U>(T arg1, U arg2);
public delegate object CallbackReturn<T, U, V>(T arg1, U arg2, V arg3);
public delegate object CallbackReturn<T, U, V,F>( T arg1, U arg2, V arg3, F arg4 );
public delegate object CallbackReturn<T, U, V, F, K>(T arg1, U arg2, V arg3, F arg4, K arg5);