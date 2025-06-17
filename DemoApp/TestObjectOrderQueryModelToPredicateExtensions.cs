using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp;

/// <summary>
/// Provides extension methods to convert TestObjectOrderQueryModel to predicates.
/// </summary>
public static class TestObjectOrderQueryModelToPredicateExtensions
{
    /// <summary>
    /// Converts the query model to a filter predicate for <see cref="TestObjectOrder"/> based on <see cref="TestObjectOrderQueryModel"/>.
    /// If any advanced filter properties are set, it will go through all versions; any selection on relevant versions should be applied before the predicate.
    /// </summary>
    public static Expression<Func<TestObjectOrder, bool>> ToPredicate(this TestObjectOrderQueryModel? query)
    {
        Expression<Func<TestObjectOrder, bool>> predicate = _ => true;
        if (query == null || !query.HasAnyFilterSet()) return predicate;

        return predicate.DecorateTestObjectOrderPredicate(query)
            .DecprateLegacyVariantPredicate(query)
            .DecorateVariantSpecificationPredicate(query);
    }

    private static Expression<Func<TestObjectOrder, bool>> DecorateTestObjectOrderPredicate(
        this Expression<Func<TestObjectOrder, bool>> predicate,
        TestObjectOrderQueryModel query)
        => predicate
            .AndIfStringPair(query.Project, query.Project, p => p.Project.ToUpper() == query.Project!.ToUpper())
            .AndIfStringPair(query.Series, query.Series, p => p.Series.ToUpper() == query.Series!.ToUpper())
            .AndIfStringPair(query.Status, query.Status, p => p.Status.ToUpper() == query.Status!.ToUpper())
            .AndIfStringPair(query.TestObjectOrderId, query.TestObjectOrderId,
                p => p.TestObjectOrderId == query.TestObjectOrderId)
            .AndIfStringPair(query.Vin, query.Vin, p => p.Vin.ToUpper() == query.Vin!.ToUpper())
            .AndIfStringPair(query.TestObjectType, query.TestObjectType,
                p => p.TestObjectType.ToUpper() == query.TestObjectType!.ToUpper());

    private static Expression<Func<TestObjectOrder, bool>> DecprateLegacyVariantPredicate(
        this Expression<Func<TestObjectOrder, bool>> predicate,
        TestObjectOrderQueryModel query)
        => predicate
            .AndIfString(query.Variant, p =>
                p.LegacyVariantSpecificationVersions
                    .OrderByDescending(v => v.CreatedOn)
                    .First().LegacyVariantSpecifications
                    .Any(spec => (spec.VariantFamily + spec.VariantNumber).ToUpper() == query.Variant!.ToUpper()))
            .AndIfString(query.Designation, p =>
                p.LegacyVariantSpecificationVersions
                    .OrderByDescending(v => v.CreatedOn)
                    .First().LegacyVariantSpecifications
                    .Any(spec => spec.VariantDesignation.ToUpper() == query.Designation!.ToUpper()));

    private static Expression<Func<TestObjectOrder, bool>> DecorateVariantSpecificationPredicate(
        this Expression<Func<TestObjectOrder, bool>> predicate,
        TestObjectOrderQueryModel query)
    => predicate
        .AndIfStringPair(query.VariableCode, query.ValueCode, p =>
            p.VariantSpecificationVersions
                .OrderByDescending(v => v.CreatedOn)
                .First()
                .VariantSpecification
                .VariantSpecificationAssignments
                .Any(ass =>
                    ass.VariableCode.ToUpper() == query.VariableCode!.ToUpper()
                    && ass.ValueCode.ToUpper() == query.ValueCode!.ToUpper()))
        .AndIf(!string.IsNullOrWhiteSpace(query.VariableCode) && string.IsNullOrWhiteSpace(query.ValueCode),
            p => p.VariantSpecificationVersions
                .OrderByDescending(v => v.CreatedOn)
                .First()
                .VariantSpecification
                .VariantSpecificationAssignments
                .Any(ass => ass.VariableCode.ToUpper() == query.VariableCode!.ToUpper()))
        .AndIf(string.IsNullOrWhiteSpace(query.VariableCode) && !string.IsNullOrWhiteSpace(query.ValueCode), p =>
            p.VariantSpecificationVersions
                .OrderByDescending(v => v.CreatedOn)
                .First()
                .VariantSpecification
                .VariantSpecificationAssignments
                .Any(ass => ass.ValueCode.ToUpper() == query.ValueCode!.ToUpper()));

}
