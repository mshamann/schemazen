Imports System
Imports Microsoft.VisualStudio.CommandBars
Imports Extensibility
Imports EnvDTE
Imports EnvDTE80
Imports System.IO
Imports System.Collections.Generic

Public Class Connect

    Implements IDTExtensibility2
    Implements IDTCommandTarget

    Private _applicationObject As DTE2
    Private _addInInstance As AddIn

    '''<summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
    Public Sub New()

    End Sub

    '''<summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
    '''<param name='application'>Root object of the host application.</param>
    '''<param name='connectMode'>Describes how the Add-in is being loaded.</param>
    '''<param name='addInInst'>Object representing this Add-in.</param>
    '''<remarks></remarks>
    Public Sub OnConnection(ByVal application As Object _
        , ByVal connectMode As ext_ConnectMode _
        , ByVal addInInst As Object _
        , ByRef custom As Array) Implements IDTExtensibility2.OnConnection

        _applicationObject = CType(application, DTE2)
        _addInInstance = CType(addInInst, AddIn)

        If connectMode = ext_ConnectMode.ext_cm_UISetup Then
            Dim commands As Commands2 = CType(_applicationObject.Commands, Commands2)
            Dim toolsMenuName As String = "Database Project"

            'Place the command on the Database Project context menu.
            Dim commandBars As CommandBars = CType(_applicationObject.CommandBars, CommandBars)
            Dim dbProjCommandBar As CommandBar = commandBars.Item("Database Project")

            'check to see if the command already exists
            Dim myCmd As Command = Nothing
            For Each c As CommandBarControl In dbProjCommandBar.Controls
                If TypeOf c Is Command AndAlso CType(c, Command).Name = "add_migration" Then
                    myCmd = CType(c, Command)
                    Exit For
                End If
            Next

            If myCmd Is Nothing Then
                'add the command
                Dim command As Command = commands.AddNamedCommand2( _
                    _addInInstance, "add_migration", "Add Migration", _
                    "Creates a new database migration.", True, 59, Nothing, _
                    CType(vsCommandStatus.vsCommandStatusSupported, Integer) _
                    + CType(vsCommandStatus.vsCommandStatusEnabled, Integer), _
                    vsCommandStyle.vsCommandStylePictAndText, _
                    vsCommandControlType.vsCommandControlTypeButton)

                'Find the appropriate command bar on the MenuBar command bar:
                command.AddControl(dbProjCommandBar, dbProjCommandBar.Controls.Count - 1)
            End If

        End If
    End Sub

    '''<summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
    '''<param name='disconnectMode'>Describes how the Add-in is being unloaded.</param>
    '''<param name='custom'>Array of parameters that are host application specific.</param>
    '''<remarks></remarks>
    Public Sub OnDisconnection(ByVal disconnectMode As ext_DisconnectMode, ByRef custom As Array) Implements IDTExtensibility2.OnDisconnection
    End Sub

    '''<summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification that the collection of Add-ins has changed.</summary>
    '''<param name='custom'>Array of parameters that are host application specific.</param>
    '''<remarks></remarks>
    Public Sub OnAddInsUpdate(ByRef custom As Array) Implements IDTExtensibility2.OnAddInsUpdate
    End Sub

    '''<summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
    '''<param name='custom'>Array of parameters that are host application specific.</param>
    '''<remarks></remarks>
    Public Sub OnStartupComplete(ByRef custom As Array) Implements IDTExtensibility2.OnStartupComplete
        Dim a As String = ""
    End Sub

    '''<summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
    '''<param name='custom'>Array of parameters that are host application specific.</param>
    '''<remarks></remarks>
    Public Sub OnBeginShutdown(ByRef custom As Array) Implements IDTExtensibility2.OnBeginShutdown
    End Sub

    '''<summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
    '''<param name='commandName'>The name of the command to determine state for.</param>
    '''<param name='neededText'>Text that is needed for the command.</param>
    '''<param name='status'>The state of the command in the user interface.</param>
    '''<param name='commandText'>Text requested by the neededText parameter.</param>
    '''<remarks></remarks>
    Public Sub QueryStatus(ByVal commandName As String, ByVal neededText As vsCommandStatusTextWanted, ByRef status As vsCommandStatus, ByRef commandText As Object) Implements IDTCommandTarget.QueryStatus
        If neededText = vsCommandStatusTextWanted.vsCommandStatusTextWantedNone Then
            If commandName = "vs_addin.Connect.add_migration" Then
                status = CType(vsCommandStatus.vsCommandStatusEnabled + vsCommandStatus.vsCommandStatusSupported, vsCommandStatus)
            Else
                status = vsCommandStatus.vsCommandStatusUnsupported
            End If
        End If
    End Sub

    '''<summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
    '''<param name='commandName'>The name of the command to execute.</param>
    '''<param name='executeOption'>Describes how the command should be run.</param>
    '''<param name='varIn'>Parameters passed from the caller to the command handler.</param>
    '''<param name='varOut'>Parameters passed from the command handler to the caller.</param>
    '''<param name='handled'>Informs the caller if the command was handled or not.</param>
    '''<remarks></remarks>
    Public Sub Exec(ByVal commandName As String, ByVal executeOption As vsCommandExecOption, ByRef varIn As Object, ByRef varOut As Object, ByRef handled As Boolean) Implements IDTCommandTarget.Exec
        handled = False
        If executeOption = vsCommandExecOption.vsCommandExecOptionDoDefault Then
            If commandName = "vs_addin.Connect.add_migration" Then
                Dim slnExplorer As UIHierarchy = CType(_applicationObject.DTE.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Object, UIHierarchy)
                Dim projectName As String = CType(slnExplorer.SelectedItems(0), UIHierarchyItem).Name
                slnExplorer.sel()

                Dim proj As Project
                For Each p As Project In _applicationObject.Solution.Projects
                    If projectName = p.Name Then
                        proj = p
                        Exit For
                    End If
                Next

                Dim slnFile As New FileInfo(_applicationObject.Solution.FullName)
                Dim projFile As New FileInfo(slnFile.DirectoryName + "\" + proj.UniqueName)
                Dim migrateDirPath As String = projFile.DirectoryName + "\migrate"
                Dim newDirName As String = Now.ToString("yyyyMMdd_hhmm")
                Dim newDirPath As String = migrateDirPath + "\" + newDirName

                If Not Directory.Exists(migrateDirPath) Then Directory.CreateDirectory(migrateDirPath)
                Directory.CreateDirectory(newDirPath)
                Directory.CreateDirectory(newDirPath + "\down")
                Directory.CreateDirectory(newDirPath + "\up")
                File.WriteAllText(newDirPath + "\down\1.sql", "--TODO your script here.")
                File.WriteAllText(newDirPath + "\up\1.sql", "--TODO your script here.")

                'HACK - can't find the API call to add a directory to a
                'database project so I'm editing the .dbp file directly."
                'the solution is closed before the project file is changed
                'then re-opened after to prevent prompting the user to reload
                _applicationObject.Solution.Close(True)
                Dim projFileText As String = File.ReadAllText(projFile.FullName)
                Dim projLines As New List(Of String)(File.ReadAllLines(projFile.FullName))
                Const MIGRATE As String = "   Begin Folder = ""migrate"""
                If Not projLines.Contains(MIGRATE) Then
                    projLines.Insert(projLines.Count - 1, MIGRATE)
                    projLines.Insert(projLines.Count - 1, "   End")
                End If

                Dim newLines As New List(Of String)
                newLines.Add("      Begin Folder = """ + newDirName + """")
                newLines.Add("         Begin Folder = ""down""")
                newLines.Add("            Script = ""1.sql""")
                newLines.Add("         End")
                newLines.Add("         Begin Folder = ""up""")
                newLines.Add("            Script = ""1.sql""")
                newLines.Add("         End")
                newLines.Add("      End")

                projLines.InsertRange(projLines.IndexOf(MIGRATE) + 1, newLines)
                File.WriteAllText(projFile.FullName, String.Join(vbCrLf, projLines.ToArray()))
                _applicationObject.Solution.Open(slnFile.FullName)

                handled = True
                Exit Sub
            End If
        End If
    End Sub
End Class
