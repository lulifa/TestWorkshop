namespace TestWorkshop;

public class SystemOrganizationUnitGetListSpecification : Specification<OrganizationUnit>
{
    protected OrganizationUnitGetByPagedDto Input { get; }
    public SystemOrganizationUnitGetListSpecification(OrganizationUnitGetByPagedDto input)
    {
        Input = input;
    }

    public override Expression<Func<OrganizationUnit, bool>> ToExpression()
    {
        Expression<Func<OrganizationUnit, bool>> expression = _ => true;

        return expression
            .AndIf(!Input.Filter.IsNullOrWhiteSpace(), x =>
                x.DisplayName.Contains(Input.Filter) || x.Code.Contains(Input.Filter));
    }
}

public static class ExpressionFuncExtensions
{
    public static Expression<Func<T, bool>> AndIf<T>(
        this Expression<Func<T, bool>> first,
        bool condition,
        Expression<Func<T, bool>> second)
    {
        if (condition)
        {
            return ExpressionFuncExtender.And(first, second);
        }

        return first;
    }

    public static Expression<Func<T, bool>> OrIf<T>(
        this Expression<Func<T, bool>> first,
        bool condition,
        Expression<Func<T, bool>> second)
    {
        if (condition)
        {
            return ExpressionFuncExtender.Or(first, second);
        }

        return first;
    }
}
