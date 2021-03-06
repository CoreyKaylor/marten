using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Marten.Services;
using Marten.Testing.Fixtures;
using Shouldly;

namespace Marten.Testing.Services
{
    public class DirtyTrackingIdentityMapTests
    {
        public void get_value_on_first_request()
        {
            var target = Target.Random();

            var serializer = new JilSerializer();

            var map = new DirtyTrackingIdentityMap(serializer);

            var target2 = map.Get<Target>(target.Id, serializer.ToJson(target));

            target2.Id.ShouldBe(target.Id);
            target2.ShouldNotBeTheSameAs(target);
        }

        public void get_value_on_subsequent_requests()
        {
            var target = Target.Random();

            var serializer = new JilSerializer();

            var map = new DirtyTrackingIdentityMap(serializer);

            var target2 = map.Get<Target>(target.Id, serializer.ToJson(target));
            var target3 = map.Get<Target>(target.Id, serializer.ToJson(target));
            var target4 = map.Get<Target>(target.Id, serializer.ToJson(target));
            var target5 = map.Get<Target>(target.Id, serializer.ToJson(target));

            target2.Id.ShouldBe(target.Id);
            target3.Id.ShouldBe(target.Id);
            target4.Id.ShouldBe(target.Id);
            target5.Id.ShouldBe(target.Id);

            target2.ShouldBeTheSameAs(target3);
            target2.ShouldBeTheSameAs(target4);
            target2.ShouldBeTheSameAs(target5);
        }

        public void get_value_on_first_request_with_lazy_json()
        {
            var target = Target.Random();

            var serializer = new JilSerializer();

            var map = new DirtyTrackingIdentityMap(serializer);

            var target2 = map.Get<Target>(target.Id, () => serializer.ToJson(target));

            target2.Id.ShouldBe(target.Id);
            target2.ShouldNotBeTheSameAs(target);
        }

        public void get_value_on_subsequent_requests_with_lazy_json()
        {
            var target = Target.Random();

            var serializer = new JilSerializer();

            var map = new DirtyTrackingIdentityMap(serializer);

            var target2 = map.Get<Target>(target.Id, () => serializer.ToJson(target));
            var target3 = map.Get<Target>(target.Id, () => serializer.ToJson(target));
            var target4 = map.Get<Target>(target.Id, () => serializer.ToJson(target));
            var target5 = map.Get<Target>(target.Id, () => serializer.ToJson(target));

            target2.Id.ShouldBe(target.Id);
            target3.Id.ShouldBe(target.Id);
            target4.Id.ShouldBe(target.Id);
            target5.Id.ShouldBe(target.Id);

            target2.ShouldBeTheSameAs(target3);
            target2.ShouldBeTheSameAs(target4);
            target2.ShouldBeTheSameAs(target5);
        }

        public void detect_changes_with_no_changes()
        {
            var a = Target.Random();
            var b = Target.Random();
            var c = Target.Random();
            var d = Target.Random();

            var serializer = new JilSerializer();

            var map = new DirtyTrackingIdentityMap(serializer);


            var a1 = map.Get<Target>(a.Id, serializer.ToJson(a));
            var b1 = map.Get<Target>(a.Id, serializer.ToJson(b));
            var c1 = map.Get<Target>(a.Id, serializer.ToJson(c));
            var d1 = map.Get<Target>(a.Id, serializer.ToJson(d));

            // no changes

            map.DetectChanges().Any().ShouldBeFalse();
        }


        public void detect_changes_with_multiple_dirties()
        {
            var a = Target.Random();
            var b = Target.Random();
            var c = Target.Random();
            var d = Target.Random();

            var serializer = new JilSerializer();

            var map = new DirtyTrackingIdentityMap(serializer);


            var a1 = map.Get<Target>(a.Id, serializer.ToJson(a));
            a1.Long++;

            var b1 = map.Get<Target>(b.Id, serializer.ToJson(b));
            var c1 = map.Get<Target>(c.Id, serializer.ToJson(c));
            c1.Long++;

            var d1 = map.Get<Target>(d.Id, serializer.ToJson(d));


            var changes = map.DetectChanges();
            changes.Count().ShouldBe(2);
            changes.Any(x => x.Id.As<Guid>() == a1.Id).ShouldBeTrue();
            changes.Any(x => x.Id.As<Guid>() == c1.Id).ShouldBeTrue();
        }

        public void detect_changes_then_clear_the_changes()
        {
            var a = Target.Random();
            var b = Target.Random();
            var c = Target.Random();
            var d = Target.Random();

            var serializer = new JilSerializer();

            var map = new DirtyTrackingIdentityMap(serializer);


            var a1 = map.Get<Target>(a.Id, serializer.ToJson(a));
            a1.Long++;

            var b1 = map.Get<Target>(b.Id, serializer.ToJson(b));
            var c1 = map.Get<Target>(c.Id, serializer.ToJson(c));
            c1.Long++;

            var d1 = map.Get<Target>(d.Id, serializer.ToJson(d));


            var changes = map.DetectChanges();

            changes.Each(x => x.ChangeCommitted());


            map.DetectChanges().Any().ShouldBeFalse();
        }

        public void remove_item()
        {
            var target = Target.Random();
            var target2 = Target.Random();
            target2.Id = target.Id;

            var serializer = new JilSerializer();

            var map = new DirtyTrackingIdentityMap(serializer);

            var target3 = map.Get<Target>(target.Id, serializer.ToJson(target));

            // now remove it
            map.Remove<Target>(target.Id);

            var target4 = map.Get<Target>(target.Id, serializer.ToJson(target2));
            target4.ShouldNotBeNull();
            target4.ShouldNotBeTheSameAs(target3);

        }
        public void store()
        {
            var target = Target.Random();
            var serializer = new JilSerializer();

            var map = new DirtyTrackingIdentityMap(serializer);

            map.Store(target.Id, target);


            map.Get<Target>(target.Id, "").ShouldBeTheSameAs(target);
        }

        public void get_with_miss_in_database()
        {
            var serializer = new JilSerializer();

            var map = new DirtyTrackingIdentityMap(serializer);
            map.Get<Target>(Guid.NewGuid(), () => null).ShouldBeNull();
        }

        public void has_positive()
        {
            var target = Target.Random();
            var serializer = new JilSerializer();

            var map = new DirtyTrackingIdentityMap(serializer);

            map.Store(target.Id, target);

            map.Has<Target>(target.Id).ShouldBeTrue();

        }

        public void has_negative()
        {
            var serializer = new JilSerializer();

            var map = new DirtyTrackingIdentityMap(serializer);
            map.Has<Target>(Guid.NewGuid()).ShouldBeFalse();
        }

        public void retrieve()
        {
            var target = Target.Random();
            var serializer = new JilSerializer();

            var map = new DirtyTrackingIdentityMap(serializer);

            map.Store(target.Id, target);

            map.Retrieve<Target>(target.Id).ShouldBeTheSameAs(target);

        }

    }
}