using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using AlphaRap;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;
using NPSDK;
using NPSDK.Motion;

namespace DynamicHardwareFixtures
{
    public class ProjectAxis : NP_Motor
    {
        public static int Created;
        /// <summary>Count construction so catalogue browsing can prove it is side-effect free.</summary>
        public ProjectAxis() { Created++; }
        /// <summary>Provide one ordinary business method without metadata registration.</summary>
        public double OffsetPosition(double origin, double offset) { return origin + offset; }
    }

    public class ProjectInput : NP_Input
    {
        /// <summary>Provide one ordinary typed input helper without physical access.</summary>
        public bool CheckInput(bool value) { return value; }
    }

    public class ProjectOutput : NP_Output
    {
        /// <summary>Provide one ordinary typed output helper without physical access.</summary>
        public int CheckOutput(int value) { return value + 1; }
    }

    public abstract class AbstractAxis : NP_Motor { }

    public class NamedAxis : NP_Motor
    {
        /// <summary>Represent a type that cannot be created by the dynamic hardware page.</summary>
        public NamedAxis(string name) { Name = name; }
    }

    public class RejectingAxis : NP_Motor
    {
        public new double WorkSpeed
        {
            get { return 10; }
            set { throw new InvalidOperationException("Rejected work speed."); }
        }
    }
}

internal static partial class Cases
{
    /// <summary>Register dynamic inherited-control selection and persistence checks.</summary>
    private static void RegisterHardwareDerivedTypeCases()
    {
        Test("dynamic hardware discovers public parameterless derived types without construction and preserves language drafts", HardwareDerivedTypeChoices);
        Test("dynamic axis and IO create the selected real derived instances and reload their stable types and point tables", HardwareDerivedTypeReload);
        Test("changing a dynamic control type preserves references leases and the original instance on failure", HardwareDerivedTypeGuards);
    }

    /// <summary>Only matching visible concrete controls are selectable; opening the list does not instantiate drivers.</summary>
    private static void HardwareDerivedTypeChoices()
    {
        DynamicHardwareFixtures.ProjectAxis.Created = 0;
        Type[] axes = HardwareControlTypes.Available("Axis");
        Assert(axes.Contains(typeof(DynamicHardwareFixtures.ProjectAxis))
            && !axes.Contains(typeof(DynamicHardwareFixtures.ProjectInput))
            && !axes.Contains(typeof(DynamicHardwareFixtures.AbstractAxis))
            && !axes.Contains(typeof(DynamicHardwareFixtures.NamedAxis))
            && DynamicHardwareFixtures.ProjectAxis.Created == 0, "The type catalogue mixed directions or constructed a control.");
        Assert(HardwareControlTypes.Available("Input").Contains(typeof(DynamicHardwareFixtures.ProjectInput))
            && HardwareControlTypes.Available("Output").Contains(typeof(DynamicHardwareFixtures.ProjectOutput)), "Derived IO types were not discovered.");
        string identity = HardwareControlTypes.Identity(typeof(DynamicHardwareFixtures.ProjectAxis));
        Assert(identity == typeof(DynamicHardwareFixtures.ProjectAxis).Assembly.GetName().Name + "|" + typeof(DynamicHardwareFixtures.ProjectAxis).FullName
            && HardwareControlTypes.Resolve("Axis", "") == typeof(NP_Motor), "Control identity includes a version or changed the empty default.");
        using (HardwareEditForm dialog = new HardwareEditForm(new HardwareDefinition { ControlType=identity }, value => { }))
        {
            DataGridView grid = (DataGridView)dialog.Controls.Find("hardwareParameters",true)[0];
            DataGridViewRow row = PropertyRow(grid,"ControlType");
            Assert(row.Cells[1] is DataGridViewComboBoxCell && Convert.ToString(row.Cells[1].Value) == identity, "The control type is not an actual selectable property.");
            foreach (LanguageType language in new[] { LanguageType.Chinese,LanguageType.English,LanguageType.Español })
            {
                SwitchLanguage(language); Application.DoEvents();
                Assert(ReferenceEquals(row,PropertyRow(grid,"ControlType")) && Convert.ToString(row.Cells[1].Value) == identity, "Language switching replaced the selected control type.");
            }
        }
        SwitchLanguage(LanguageType.Chinese);
        bool rejected = false;
        try { HardwareControlTypes.Resolve("Input",identity); } catch (InvalidOperationException) { rejected = true; }
        Assert(rejected, "An axis type was accepted for an input.");
        rejected = false;
        try { new HardwareDefinition { ControlType="MissingAssembly|Missing.Axis" }.Validate(); } catch (InvalidOperationException) { rejected = true; }
        Assert(rejected, "A missing saved type silently became a standard axis.");
    }

    /// <summary>Exercise real runtime creation and XML reload, preserving the selected control and its bound recipe table.</summary>
    private static void HardwareDerivedTypeReload()
    {
        HardwareDefinition[] definitions = {
            hardware.Resources.NewDefinition("Axis"),
            hardware.Resources.NewDefinition("Input"),
            hardware.Resources.NewDefinition("Output")
        };
        Type[] types = { typeof(DynamicHardwareFixtures.ProjectAxis),typeof(DynamicHardwareFixtures.ProjectInput),typeof(DynamicHardwareFixtures.ProjectOutput) };
        string[] methods = { "OffsetPosition","CheckInput","CheckOutput" };
        ControlBaseInterface[] originals = new ControlBaseInterface[3];
        try
        {
            for (int index=0;index<definitions.Length;index++)
            {
                HardwareDefinition definition = definitions[index];
                definition.ControlType = HardwareControlTypes.Identity(types[index]);
                HardwareEntry entry = hardware.Resources.Put(definition); originals[index]=entry.Device;
                Assert(entry.Device.GetType() == types[index] && ReferenceEquals(entry.Device.Parent,entry.Group)
                    && !entry.Device.InitializeWithSdk, "The selected derived control was not the owned actual hardware instance.");
                if (index == 0)
                {
                    NP_AxisGroup group = (NP_AxisGroup)entry.Group;
                    group.Points.DataSet1.Rows.Add(37.5,"Derived point");
                    Assert(ReferenceEquals(group.Motor,group.Points.WorkMotor) && ReferenceEquals(group.Motor,group.Points.ParameterSet.Single().Axis), "The selected axis did not own its bound points.");
                }
                else Assert(ReferenceEquals(((NP_IoGroup)entry.Group).IO,entry.Device), "The IO template still exposes a hidden standard instance.");
            }
            hardware.RecipeData.AcceptChanges(); hardware.Resources.Save();
            foreach (HardwareDefinition definition in definitions)
                Assert(HardwareResources.Load(hardware.Resources.PathName).Resources.Single(item=>item.Id==definition.Id).ControlType == definition.ControlType, "Main-save DTO lost the selected control identity.");
            hardware.Resources.DiscardChanges();
            FlowActions actions = new FlowActions(FlowResources.Capture());
            for (int index=0;index<definitions.Length;index++)
            {
                HardwareEntry entry = hardware.Resources.Entries.Single(item=>item.Definition.Id==definitions[index].Id);
                FlowResource resource = actions.Resources.Items.Values.Single(item=>ReferenceEquals(item.Instance,entry.Device));
                Assert(entry.Device.GetType()==types[index] && originals[index].IsDisposed && !SDKPara.ControlsSnapshot().Contains(originals[index])
                    && resource.Methods.Any(method=>method.Id.StartsWith("auto.",StringComparison.Ordinal) && method.Title[0].Contains(methods[index])), "Reload lost a type, retained a disposed driver, or omitted its inherited tool.");
                if (index==0) Assert(((NP_AxisGroup)entry.Group).Points.DataSet1.Rows[0].Field<double>("Position")==37.5, "Type reload replaced the bound recipe point table.");
            }
        }
        finally
        {
            foreach (HardwareDefinition definition in definitions)
                if (hardware.Resources.Entries.Any(item=>item.Definition.Id==definition.Id)) hardware.Resources.Delete(definition.Id);
            hardware.Resources.Save();
        }
    }

    /// <summary>Changing implementation is a guarded replacement, and failed candidate creation leaves the original instance intact.</summary>
    private static void HardwareDerivedTypeGuards()
    {
        HardwareDefinition definition = hardware.Resources.NewDefinition("Axis");
        definition.ControlType = HardwareControlTypes.Identity(typeof(DynamicHardwareFixtures.ProjectAxis));
        HardwareEntry original = hardware.Resources.Put(definition);
        HardwareDefinition replacement = definition.Copy(); replacement.ControlType="";
        try
        {
            using (FlowResourceGate.Lease lease = FlowResourceGate.Acquire(new object[]{original.Device},CancellationToken.None).GetAwaiter().GetResult())
            {
                bool rejected=false;
                try { hardware.Resources.Put(replacement); } catch (InvalidOperationException) { rejected=true; }
                Assert(rejected && ReferenceEquals(original,hardware.Resources.Entries.Single(item=>item.Definition.Id==definition.Id)), "A leased control changed implementation.");
            }
            using (Flow_BaseForm form = FlowPrograms.AddModule("Derived control consumer"))
            {
                FlowModuleEditor module = FlowPrograms.Attach(form); FlowStep step = module.AddNode();
                step.Target="hardware/"+definition.Id+"/axis";
                bool rejected=false;
                try { hardware.Resources.Put(replacement); } catch (InvalidOperationException) { rejected=true; }
                Assert(rejected && !original.Device.IsDisposed, "A referenced implementation changed and silently broke a node.");
                module.Delete(step.Id); FlowPrograms.DeleteModule(module);
            }
            HardwareDefinition invalid = definition.Copy(); invalid.ControlType=HardwareControlTypes.Identity(typeof(DynamicHardwareFixtures.RejectingAxis));
            bool failed=false;
            try { hardware.Resources.Put(invalid); } catch (Exception) { failed=true; }
            Assert(failed && ReferenceEquals(original,hardware.Resources.Entries.Single(item=>item.Definition.Id==definition.Id))
                && !original.Device.IsDisposed && !SDKPara.ControlsSnapshot().Any(item=>item.GetType()==typeof(DynamicHardwareFixtures.RejectingAxis)), "A failed candidate disposed the original or leaked its replacement.");
            HardwareEntry changed = hardware.Resources.Put(replacement);
            Assert(changed.Device.GetType()==typeof(NP_Motor) && original.Device.IsDisposed, "An unused idle control could not change back to its standard type.");
        }
        finally { hardware.Resources.Delete(definition.Id); hardware.Resources.Save(); }
    }
}
