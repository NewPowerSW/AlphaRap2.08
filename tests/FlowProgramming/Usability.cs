using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaRap;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;
using AlphaRapLibrary;
using NPSDK;
using NPSDK.Motion;

internal sealed class CardOutputDriver : IOBaseInterface
{
    internal int Writes;
    private bool state = true;
    /// <summary>Expose a fake electrical state without any hardware access.</summary>
    public override bool GetState() { return state; }
    /// <summary>Record one electrical high command.</summary>
    public override bool On() { Writes++; state=true; return true; }
    /// <summary>Record one electrical low command.</summary>
    public override bool Off() { Writes++; state=false; return true; }
}

internal static partial class Cases
{
    /// <summary>Render the delivered window or panel as a regression artifact.</summary>
    private static void Screenshot(Control control,string name)
    {
        using (Bitmap bitmap = new Bitmap(control.Width,control.Height)) { control.DrawToBitmap(bitmap,new Rectangle(Point.Empty,control.Size)); bitmap.Save(Path.Combine(root,name+".png")); }
    }

    /// <summary>Flat IO cards reuse the actual object; manual toggles respect host state and leases.</summary>
    private static void CompactIoCards()
    {
        using (NP_OutputGroup group = new NP_OutputGroup())
        {
            CardOutputDriver driver = new CardOutputDriver(); NP_Output output=(NP_Output)group.IO;
            typeof(NP_Output).GetField("dOutput",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(output,driver);
            typeof(NP_Output).GetField("ready",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(output,true);
            MethodInfo click=typeof(Control).GetMethod("OnClick",BindingFlags.Instance|BindingFlags.NonPublic);
            group.CanOperate=()=>false; click.Invoke(output,new object[]{EventArgs.Empty}); Assert(driver.Writes==0,"A card bypassed the host permission guard.");
            group.CanOperate=()=>true; click.Invoke(output,new object[]{EventArgs.Empty}); Assert(driver.Writes==1,"An allowed card click did not toggle exactly once.");
            click.Invoke(output,new object[]{new MouseEventArgs(MouseButtons.Right,1,5,5,0)}); Assert(driver.Writes==1,"Opening output properties with a right-click toggled the signal.");
            using (FlowResourceGate.Lease lease=FlowResourceGate.Acquire(new object[]{output},CancellationToken.None).GetAwaiter().GetResult())
                click.Invoke(output,new object[]{EventArgs.Empty});
            Assert(driver.Writes==1,"A leased output was manually toggled.");
            Assert(group.Controls.Count==1 && group.Height==40 && group.BorderStyle==BorderStyle.None && group.Controls.OfType<Button>().Count()==0,"IO still has a nested frame or parameter buttons.");
        }
        List<string> additions=new List<string>();
        foreach(string kind in new[]{"Input","Input","Output","Output","Output"}){HardwareDefinition definition=hardware.Resources.NewDefinition(kind);hardware.Resources.Put(definition);additions.Add(definition.Id);}
        ioPage.Show(); ((TabControl)ioPage.Controls.Find("tabControl1",true)[0]).SelectedTab=(TabPage)ioPage.Controls.Find("ioResourcesPage",true)[0]; Application.DoEvents();
        HardwareResourcePanel panel=(HardwareResourcePanel)ioPage.Controls.Find("ioResourcePanel",true)[0];
        Assert(!panel.Controls.Find("resourceSelected",true)[0].Visible && !panel.Controls.Find("resourceEdit",true)[0].Visible,"IO still has duplicate resource selectors.");
        Assert(hardware.Resources.Entries.Where(entry=>entry.Definition.Kind!="Axis").All(entry=>entry.Group.Height==40),"IO was not compact after host binding.");
        Control canvas=panel.Controls.Find("resourceCanvas",true)[0];Assert(canvas.Controls.Find("inputHeader_dyn",true)[0].Top<canvas.Controls.Find("outputHeader_dyn",true)[0].Top,"Inputs and outputs were not grouped in the correct order.");
        Screenshot(panel,"compact-io-cards"); ioPage.Hide();foreach(string id in additions)hardware.Resources.Delete(id);hardware.Resources.Save();
    }

    /// <summary>Direct operand selection preserves stable sources, literal drafts, comparison codes and condition groups.</summary>
    private static void DirectConditionEditing()
    {
        SwitchLanguage(LanguageType.Chinese);
        FlowResource input=FlowResources.Capture().Items["hardware/"+groupedInput.Id+"/input"];
        FlowStep step=BoundStep(input,"io.read");step.Caption="后门到位判断";
        using (FlowNodeForm dialog=new FlowNodeForm(editor,step))
        {
            dialog.Show(); Application.DoEvents();
            TabControl tabs = (TabControl)dialog.Controls.Find("executionTabs",true)[0]; tabs.SelectedTab = tabs.TabPages["scriptRoutes"]; Application.DoEvents();
            FlowBranchEditor branch=(FlowBranchEditor)dialog.Controls.Find("branch_NEXT_dyn",true)[0];
            ((RadioButton)branch.Controls.Find("branchAll",true)[0]).Checked=true;
            FlowConditionRow row=branch.ConditionRows.Single(); ComboBox left=(ComboBox)row.Controls.Find("conditionLeft",true)[0], right=(ComboBox)row.Controls.Find("conditionRight",true)[0], comparison=(ComboBox)row.Controls.Find("conditionComparison",true)[0];
            Assert(row.ReadCondition().Left.Source=="Result" && row.ReadCondition().Right.Reference=="true","A boolean result did not default to a usable true condition.");
            right.SelectedItem=right.Items.Cast<object>().First(item=>item.ToString().Contains("Setting / DisScann"));
            Assert(row.ReadCondition().Right.Source=="Parameter" && row.ReadCondition().Right.Reference==ConstantStore.Reference("Setting","DisScann"),"Selecting a constant saved a display label instead of its stable identity.");
            right.SelectedItem=right.Items.Cast<object>().First(item=>Convert.ToString(item.GetType().GetField("Reference",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(item))==editor.Program.ModuleId+"/"+model.Id+"/Value");
            Assert(row.ReadCondition().Right.Source=="Output","A direct node-output choice became a literal.");
            comparison.SelectedValue="Empty"; Assert(!right.Enabled,"Empty comparison still asks for a right operand.");
            comparison.SelectedValue="=="; right.Focus(); right.Text="pending 1.";
            foreach(LanguageType language in new[]{LanguageType.Chinese,LanguageType.English,LanguageType.Español})
            {
                SwitchLanguage(language); Application.DoEvents();
                Assert(row.ReadCondition().Left.Source=="Result" && right.Text=="pending 1." && Convert.ToString(comparison.SelectedValue)=="==","Translation changed a source, operator or literal draft.");
                right.Text="true"; Screenshot(dialog,"direct-conditions-"+language); right.Text="pending 1.";
            }
            ((Button)branch.Controls.Find("addCondition",true)[0]).PerformClick(); Assert(branch.ConditionRows.Count()==2,"The add-condition command did not add one row.");
            FlowBranch copy=FlowProgramFile.Snapshot(new FlowProgram{Steps=new List<FlowStep>{new FlowStep{Branches=new List<FlowBranch>{branch.ReadBranch()}}}}).Steps[0].Branches[0];
            Assert(copy.Conditions.Count==2 && copy.Conditions[0].Left.Reference=="Value" && copy.Conditions[0].Right.Reference=="pending 1.","Direct rows did not round-trip through the existing model.");
            dialog.Close();
        }
        SwitchLanguage(LanguageType.Chinese);
    }

    /// <summary>Flow writes change the real typed table while preserving disk and Original until the main Save.</summary>
    private static void ConstantRuntimeWrites()
    {
        ConstantStore store=constantForm.Constants; string key="Writable run number", integer="Writable run integer";
        store.Put("Setting",null,new ConstantValue{Key=key,Type="Number",Value="2.5"});
        store.Put("Recipe",null,new ConstantValue{Key=integer,Type="Integer",Value="2"}); store.Save("Setting"); store.Save("Recipe");
        FlowActions actions=new FlowActions(FlowResources.Capture()); FlowResultHub hub=new FlowResultHub();
        FlowExecutionContext first=new FlowExecutionContext{Hub=hub,RunId=hub.Epoch,Parameters=actions.Resources.Parameters,ModuleId="Writer",CycleId="A"};
        FlowExecutionContext second=new FlowExecutionContext{Hub=hub,RunId=hub.Epoch,Parameters=actions.Resources.Parameters,ModuleId="Reader",CycleId="B"};
        FlowStep write=BoundStep(actions.FindResource("constants/Setting"),"constant.set"); write.Inputs[0].Reference=key; write.Inputs[1].Reference="7.25";
        actions.Execute(write,first,CancellationToken.None).GetAwaiter().GetResult();
        FlowStep read=BoundStep(actions.FindResource("constants/Setting"),"constant.read"); read.Inputs[0].Reference=key;
        Assert((double)actions.Execute(read,second,CancellationToken.None).GetAwaiter().GetResult()["Value"]==7.25,"Another module did not observe the run value.");
        Assert((double)second.Resolve(new FlowInput{Source="Parameter",Reference=ConstantStore.Reference("Setting",key)})==7.25,"Direct condition/input bindings read a stale initial value.");
        DataRow changed=ConstantStore.Find(store.Table("Setting"),key);
        Assert(Convert.ToString(changed["Value"])=="7.25" && Convert.ToString(changed["Value",DataRowVersion.Original])=="2.5" && changed.RowState==DataRowState.Modified,"The node did not update the real table or committed it without the main Save.");
        Assert((double)actions.Resources.Parameters[ConstantStore.Reference("Setting",key)]==2.5 && (double)ConstantValueOf(new FlowActions(FlowResources.Capture()),"Setting",key)==7.25,"Fresh flows cannot read the table value or an old captured parameter snapshot was mutated.");
        FlowStep bad=BoundStep(actions.FindResource("constants/Recipe"),"constant.set"); bad.Inputs[0].Reference=integer; bad.Inputs[1].Reference="1.5";
        bool blocked=false; try{actions.Execute(bad,first,CancellationToken.None).GetAwaiter().GetResult();}catch(InvalidOperationException){blocked=true;} Assert(blocked,"An integer constant rounded a fractional write.");
        hub.Reset(); blocked=false; try{actions.Execute(write,first,CancellationToken.None).GetAwaiter().GetResult();}catch(OperationCanceledException){blocked=true;} Assert(blocked,"An expired run wrote to the new batch.");
        second.RunId=hub.Epoch; Assert((double)actions.Execute(read,second,CancellationToken.None).GetAwaiter().GetResult()["Value"]==7.25,"A new batch reverted the actual Setting table value.");
        store.Put("Setting",key,new ConstantValue{Key=key,Type="Number",Value="9"});
        Assert((double)ConstantValueOf(new FlowActions(FlowResources.Capture()),"Setting",key)==7.25,"An ordinary page draft replaced the last value explicitly set by a flow.");
        store.Table("Setting").RejectChanges();
        Assert((double)ConstantValueOf(new FlowActions(FlowResources.Capture()),"Setting",key)==2.5,"Discard did not restore the saved table value.");
        FlowStep consumer=editor.AddNode(); consumer.Action="constant.set"; consumer.Target="constants/Setting"; consumer.Inputs=write.Inputs;
        blocked=false;try{store.Delete("Setting",key);}catch(InvalidOperationException){blocked=true;}Assert(blocked,"A set-node key was deleted while referenced.");
        editor.Delete(consumer.Id); store.Delete("Setting",key);store.Save("Setting");store.Delete("Recipe",integer);store.Save("Recipe");
    }

    /// <summary>Background setters marshal to the table's UI thread and reject cancelled or stale queued writes.</summary>
    private static void ConstantTableWriteDispatch()
    {
        ConstantStore store=constantForm.Constants; string key="Dispatched table write";
        store.Put("Setting",null,new ConstantValue{Key=key,Type="Number",Value="1"}); store.Save("Setting");
        FlowActions actions=new FlowActions(FlowResources.Capture()); FlowResultHub hub=new FlowResultHub();
        FlowExecutionContext context=new FlowExecutionContext{Hub=hub,RunId=hub.Epoch,Parameters=actions.Resources.Parameters};
        FlowStep write=BoundStep(actions.FindResource("constants/Setting"),"constant.set");write.Inputs[0].Reference=key;write.Inputs[1].Reference="4.5";
        int changedThread=0,expectedThread=Thread.CurrentThread.ManagedThreadId;
        DataColumnChangeEventHandler observed=(sender,args)=>{if(Convert.ToString(args.Row["Key"])==key)changedThread=Thread.CurrentThread.ManagedThreadId;};
        store.Table("Setting").ColumnChanged+=observed;
        try
        {
            Task task=Task.Run(()=>actions.Execute(write,context,CancellationToken.None)); Wait(task);
            Assert(changedThread==expectedThread && Convert.ToString(ConstantStore.Find(store.Table("Setting"),key)["Value"])=="4.5","A flow wrote the bound table from a worker thread.");
            using(CancellationTokenSource cancellation=new CancellationTokenSource())
            using(ManualResetEventSlim posted=new ManualResetEventSlim())
            {
                Task pending=null;write.Inputs[1].Reference="9";
                Task launch=Task.Run(()=>{pending=actions.Execute(write,context,cancellation.Token);posted.Set();});
                Assert(posted.Wait(2000),"The table write was not queued."); cancellation.Cancel();Wait(launch);
                bool cancelled=false;try{Wait(pending);}catch(OperationCanceledException){cancelled=true;}
                Assert(cancelled && Convert.ToString(ConstantStore.Find(store.Table("Setting"),key)["Value"])=="4.5","A cancelled queued action changed the table.");
            }
            using(ManualResetEventSlim posted=new ManualResetEventSlim())
            {
                Task pending=null; Task launch=Task.Run(()=>{pending=actions.Execute(write,context,CancellationToken.None);posted.Set();});
                Assert(posted.Wait(2000),"The old-batch write was not queued.");hub.Reset();Wait(launch);
                bool cancelled=false;try{Wait(pending);}catch(OperationCanceledException){cancelled=true;}
                Assert(cancelled && Convert.ToString(ConstantStore.Find(store.Table("Setting"),key)["Value"])=="4.5","An expired queued action changed the table.");
            }
        }
        finally{store.Table("Setting").ColumnChanged-=observed;store.Table("Setting").RejectChanges();store.Delete("Setting",key);store.Save("Setting");}
    }

    /// <summary>All valid rows of the selected axis appear in the picker with stable zero-based indices and a visible table context.</summary>
    private static void AxisMultiplePointChoices()
    {
        NP_AxisGroup group=(NP_AxisGroup)hardware.Resources.Entries.Single(item=>item.Definition.Id==groupedAxis.Id).Group;
        DataTable table=group.Points.DataSet1; int saved=table.Rows.Count;
        table.Rows.Add(82,"Second point"); table.Rows.Add(83,"Third point"); table.Rows.Add(DBNull.Value,"Unconfigured point");
        try
        {
            FlowActions actions=new FlowActions(FlowResources.Capture(true)){Simulate=true};
            FlowResource axis=actions.FindResource("hardware/"+groupedAxis.Id+"/axis");FlowMethod move=axis.Methods.Single(method=>method.Id=="point.move");
            FlowPointAxis points=(FlowPointAxis)axis.BoundPoints.Instance;
            Assert(points.RowCount==saved+3 && move.Inputs[0].Options.Count==saved+2,"The axis picker lost rows or offered a blank coordinate.");
            Assert(new FlowActions(FlowResources.Capture()).FindResource(axis.Id).Methods.Single(method=>method.Id=="point.move").Inputs[0].Options.Count==saved,"Unsaved points leaked into the production snapshot.");
            FlowStep step=BoundStep(axis,"point.move");step.Inputs[0].Reference=(saved+1).ToString();
            Assert((double)ExecuteBound(actions,step)["Position"]==83,"The last point index was remapped to the first row.");
            using(FlowNodeForm dialog=new FlowNodeForm(editor,step))
            {
                dialog.Show();Application.DoEvents();
                foreach(LanguageType language in new[]{LanguageType.Chinese,LanguageType.English,LanguageType.Español})
                {
                    SwitchLanguage(language);Application.DoEvents();Control hint=dialog.Controls.Find("methodHint_dyn",true)[0];
                    Assert(hint.Visible && hint.Text.Contains(axis.Label) && hint.Text.Contains((saved+2).ToString()),"The movement node omitted its binding or selectable count: "+hint.Text+"; expected "+axis.Label+"; visible="+hint.Visible);
                    using(FlowSourceForm picker=new FlowSourceForm(step.Inputs[0],actions,move,move.Inputs[0],hint.Text,axis.Id))
                    {
                        picker.Show();Application.DoEvents();TreeView tree=(TreeView)picker.Controls.Find("sources",true)[0];Control context=picker.Controls.Find("sourceContext_dyn",true)[0];
                        Assert(tree.Nodes["bindingFixed"].Nodes.Count==saved+2 && context.Visible && !tree.Bounds.IntersectsWith(context.Bounds),"The picker hid points or covered the table context.");
                        tree.SelectedNode=tree.Nodes["bindingFixed"].Nodes.Cast<TreeNode>().Single(node=>node.Name=="Constant:"+(saved+1).ToString());
                        Screenshot(picker,"axis-point-picker-"+language);((Button)picker.Controls.Find("ok",true)[0]).PerformClick();
                        Assert(picker.Selection.Reference==(saved+1).ToString(),"Point selection saved a label instead of its original row index.");
                    }
                    Screenshot(dialog,"axis-point-help-"+language);
                }
                dialog.Close();
            }
        }
        finally{table.RejectChanges();SwitchLanguage(LanguageType.Chinese);}
    }

    /// <summary>VPForm exposes only configured VPPs; its unused Camera1 compatibility fields never appear as selectable vision objects.</summary>
    private static void ConfiguredVisionCatalog()
    {
        object previous=host.GetField("VPF").GetValue(null); VisionproInterface[] before=VisionproInterface.VList.ToArray();
        using(VPForm form=new VPForm())
        {
            SetHost("VPF",form); RegisterForm(form);
            try
            {
                List<VpCameraConfig> cameras=(List<VpCameraConfig>)typeof(VPForm).GetProperty("_cameras",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(form,null); cameras.Clear();
                Assert(!form.GetFlowStations().Any() && !FlowResources.Capture().Items.Values.Any(item=>item.Instance is VisionproInterface),"Unused legacy Camera1 fields leaked into the object catalog.");
                FlowVisionProbe station=new FlowVisionProbe(); VpVppConfig vpp=new VpVppConfig{Name="Configured VPP",Station=station};
                cameras.Add(new VpCameraConfig{Name="Configured camera",Vpps=new List<VpVppConfig>{vpp}});
                FlowResource actual=FlowResources.Capture().Items.Values.Single(item=>item.Kind=="vision");
                Assert(actual.Id=="vision/vpp/"+vpp.Id && ReferenceEquals(actual.Instance,station),"The configured VPP identity or instance changed.");
            }
            finally{SetHost("VPF",previous);((IList)host.GetField("lstForm").GetValue(null)).Remove(form);ModuleManager.ModuleList.Remove(form);VisionproInterface.VList.RemoveAll(item=>!before.Contains(item));}
        }
    }

    /// <summary>Each IO category lists only its own actual objects and methods, and read-only methods hide the empty parameter grid.</summary>
    private static void IoCategorySelection()
    {
        FlowActions actions=new FlowActions(FlowResources.Capture()); FlowStep step=BoundStep(actions.FindResource("hardware/"+groupedInput.Id+"/input"),"io.read");
        using(FlowNodeForm dialog=new FlowNodeForm(editor,step))
        {
            dialog.Show();Application.DoEvents();ComboBox category=(ComboBox)dialog.Controls.Find("categoryValue",true)[0], target=(ComboBox)dialog.Controls.Find("targetValue",true)[0];
            Assert(Convert.ToString(category.SelectedValue)=="di" && !dialog.Controls.Find("inputs",true)[0].Visible,"Input read still uses a mixed category or empty input grid.");
            Assert(((List<FlowChoice>)target.DataSource).All(choice=>actions.Resources.Items[choice.Id].Kind=="input"),"DI category contains other object kinds.");
            category.SelectedValue="do";Application.DoEvents();Assert(((List<FlowChoice>)target.DataSource).All(choice=>actions.Resources.Items[choice.Id].Kind=="output"),"DO category contains inputs or cylinders.");
            category.SelectedValue="cylinder";Application.DoEvents();Assert(((List<FlowChoice>)target.DataSource).All(choice=>actions.Resources.Items[choice.Id].Kind=="cylinder"),"Cylinder category is mixed with other IO.");dialog.Close();
        }
    }

    /// <summary>Tray indexing, pitch offsets and on-screen instructions agree with the actual recipe table.</summary>
    private static void TrayNodeHelp()
    {
        object previous=host.GetField("PlatF").GetValue(null);
        using(PlatFormSetting form=new PlatFormSetting())
        {
            SetHost("PlatF",form);form.ModuleInitialize("TrayHelpProbe");
            try
            {
                DataTable table=form.RecipeData.Tables["PSet"]; if(table.Rows.Count==0)table.Rows.Add(table.NewRow());
                DataRow data=table.Rows[0];data["TrayRowNum"]=3;data["TrayColumnNum"]=4;data["TrayRowdis"]=10;data["TrayColumnDis"]=20;form.RecipeData.AcceptChanges();
                FlowActions actions=new FlowActions(FlowResources.Capture());FlowResource tray=actions.FindResource("tray/Tray");FlowStep step=BoundStep(tray,"tray.position");step.Inputs[0].Reference="5";
                IDictionary<string,object> result=ExecuteBound(actions,step);Assert((int)result["Row"]==1 && (int)result["Column"]==1 && (double)result["OffsetX"]==20 && (double)result["OffsetY"]==10,"Tray indexing or pitch offsets do not match the explanation.");
                using(FlowNodeForm dialog=new FlowNodeForm(editor,step)){dialog.Show();Application.DoEvents();Control hint=dialog.Controls.Find("methodHint_dyn",true)[0];string help=hint.Text;Assert(hint.Visible && hint.Height>=40 && help.Contains("3")&&help.Contains("4")&&help.Contains("11")&&help.Contains("OffsetX"),"The tray node omits or clips its layout, index range or output usage.");Assert(!hint.Bounds.IntersectsWith(dialog.Controls.Find("inputs",true)[0].Bounds),"The input grid covers the tray instructions.");Screenshot(dialog,"tray-node-help");dialog.Close();}
            }
            finally{SetHost("PlatF",previous);form.ModuleDispose();ModuleManager.ModuleList.Remove(form);}
        }
    }
}
