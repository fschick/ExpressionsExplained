# Introduction

The ability to treat expressions as data structures enables APIs to receive user code in a format that can be inspected, transformed, and processed in a custom manner. For example, the LINQ to SQL data access implementation uses this facility to translate expression trees to Transact-SQL statements that can be evaluated by the database.

# Expression Lambda

A lambda expression with an expression on the right side of the `=>` operator is called an *expression lambda*.

```csharp
Func<int, int> square = x => x * x;

Console.WriteLine(square); // Output:
// System.Func`2[System.Int32,System.Int32]

Console.WriteLine(square(5)); // Output:
// 25
```

To create a *lambda expression*, you specify input parameters (if any) on  the left side of the lambda operator and an expression or a statement  block on the other side. Any *lambda expression* can be converted to a [delegate](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/reference-types#the-delegate-type) type.

# Statement Lambdas

A *statement lambda* resembles an *expression lambda* except that its statements are enclosed in braces. The C# compiler cannot use *statement lambdas* to create *expression trees*.

```csharp
Func<int, int> square = x =>
{
    return x * x;
};

// Outputs:
// Same as above
```

# Expression Trees

When you specify an `Expression<TDelegate>` argument, the lambda is compiled to an *expression tree*. *Expression trees* represent code in a tree-like data structure, where  each node is an expression, for example, a method call or a binary operation such as `x * x`. The C# compiler can generate *expression trees* only from *expression lambdas*, not from *expression statements*.

```csharp
Expression<Func<int, int>> squareExpr = x => x * x;

Console.WriteLine(squareExpr); // Output:
// x => (x * x)

Console.WriteLine(squareExpr(5));
// Compiler error: CS0149 Method name expected

Console.WriteLine(squareExpr.Compile()(5)); // Output:
// 25
```

# Inspect an Expressions Tree

```csharp
// Create an expression tree.  
Expression<Func<int, int>> addFiveExpr = value => value + 5;
  
// Decompose the expression tree.  
var param = (ParameterExpression)addFiveExpr.Parameters[0];
var operation = (BinaryExpression)addFiveExpr.Body;
var left = (ParameterExpression)operation.Left;
var opertor = operation.NodeType;
var right = (ConstantExpression)operation.Right;
  
Console.WriteLine($"Expression: {param.Name} => {left.Name} {opertor} {right.Value}"); //Output:
// Expression: value => value Add 5
```

# Create an Expressions Tree

Manually build the expression tree for the lambda expression `addFiveExpr = value => value + 5`.

```csharp
var value = Expression.Parameter(typeof(int), "value");
var five = Expression.Constant(5, typeof(int));
var valueAddFiveBinary = Expression.Add(value, five);

var addFiveExpr = Expression.Lambda<Func<int, int>>(body: valueAddFiveBinary, parameters: value);
Console.WriteLine(addFiveExpr); // Output:
// value => (value + 5)
```

# Links

https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/lambda-expressions

https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees/

https://docs.microsoft.com/en-us/dotnet/api/system.linq.expressions.expression-1?view=net-5.0

