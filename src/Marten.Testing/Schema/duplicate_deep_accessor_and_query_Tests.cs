﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Baseline;
using Marten.Schema;
using Marten.Services;
using Marten.Testing.Fixtures;

namespace Marten.Testing.Schema
{
    public class duplicate_deep_accessor_and_query_Tests : DocumentSessionFixture<NulloIdentityMap>
    {
        public void duplicate_and_search_off_of_deep_accessor_by_number()
        {
            var targets = Target.GenerateRandomData(10).ToArray();
            theContainer.GetInstance<IDocumentSchema>().Alter(_ =>
            {
                _.For<Target>().Searchable(x => x.Inner.Number);
            });

            targets.Each(x => theSession.Store(x));
            theSession.SaveChanges();

            var thirdTarget = targets.ElementAt(2);

            var results = theSession.Query<Target>().Where(x => x.Inner.Number == thirdTarget.Inner.Number).ToArray();
            results
                .Any(x => x.Id == thirdTarget.Id).ShouldBeTrue();


        }

        public void duplicate_and_search_off_of_deep_accessor_by_date()
        {
            var targets = Target.GenerateRandomData(10).ToArray();
            theContainer.GetInstance<IDocumentSchema>().Alter(_ =>
            {
                _.For<Target>().Searchable(x => x.Inner.Date);
            });

            targets.Each(x => theSession.Store(x));
            theSession.SaveChanges();

            var thirdTarget = targets.ElementAt(2);

            var queryable = theSession.Query<Target>().Where(x => x.Inner.Date == thirdTarget.Inner.Date);
            var results = queryable.ToArray();
            results
                .Any(x => x.Id == thirdTarget.Id).ShouldBeTrue();

            Debug.WriteLine(theStore.Diagnostics.CommandFor(queryable).CommandText);

            theStore.Diagnostics.CommandFor(queryable).CommandText
                .ShouldContain("inner_date = :arg0");
        }
    }
}