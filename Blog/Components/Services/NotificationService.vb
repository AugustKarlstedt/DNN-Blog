﻿'
' DotNetNuke® - http://www.dotnetnuke.com
' Copyright (c) 2002-2011
' by DotNetNuke Corporation
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'
Option Strict On
Option Explicit On

Imports DotNetNuke.Modules.Blog.Components.Common
Imports DotNetNuke.Modules.Blog.Components.Controllers
Imports DotNetNuke.Web.Services
Imports System.Web.Mvc
Imports DotNetNuke.Services.Social.Notifications
Imports DotNetNuke.Modules.Blog.Components.Entities

Namespace Components.Services
    Public Class NotificationServiceController
        Inherits DnnController

#Region "Private Members"

        Private TabId As Integer = -1
        Private ModuleId As Integer = -1
        Private BlogId As Integer = -1
        Private EntryId As Integer = -1

#End Region

        <DnnAuthorize()> _
        Public Function ApprovePost(notificationId As Integer) As ActionResult
            Dim notify As Notification = NotificationsController.Instance.GetNotification(notificationId)
            ParseKey(notify.Context)

            Dim cntBlog As New BlogController
            Dim objBlog As BlogInfo = cntBlog.GetBlog(BlogId)

            If objBlog Is Nothing Then
                Return Json(New With {.Result = "error"})
            End If

            If objBlog.AuthorMode = Constants.AuthorMode.PersonalMode Then
                ' this should never happen
                Return Json(New With {.Result = "error"})
            ElseIf objBlog.AuthorMode = Constants.AuthorMode.GhostMode Then
                Dim isOwner As Boolean = objBlog.UserID = UserInfo.UserID

                ' NOTE: we need to allow more than just the owner (think of admin)
                If Not isOwner Then
                    Return Json(New With {.Result = "error"})
                End If

                ' approve the blog post
                Dim cntEntry As New EntryController
                Dim objEntry As EntryInfo = cntEntry.GetEntry(EntryId, PortalSettings.PortalId)

                If objEntry Is Nothing Then
                    Return Json(New With {.Result = "error"})
                End If

                objEntry.Published = True
                cntEntry.UpdateEntry(objEntry, TabId, PortalSettings.PortalId)
            Else
                ' blogger mode
                Dim isOwner As Boolean = objBlog.UserID = UserInfo.UserID
                Dim objSecurity As New ModuleSecurity(ModuleId, TabId)

                If objSecurity.CanAddEntry(isOwner, Constants.AuthorMode.BloggerMode) Then
                    ' approve the blog post
                    Dim cntEntry As New EntryController
                    Dim objEntry As EntryInfo = cntEntry.GetEntry(EntryId, PortalSettings.PortalId)

                    If objEntry Is Nothing Then
                        Return Json(New With {.Result = "error"})
                    End If

                    objEntry.Published = True
                    cntEntry.UpdateEntry(objEntry, TabId, PortalSettings.PortalId)
                Else
                    Return Json(New With {.Result = "error"})
                End If
            End If

            NotificationsController.Instance().DeleteNotification(notificationId)
            Return Json(New With {.Result = "success"})
        End Function

        <DnnAuthorize()> _
        Public Function DeletePost(notificationId As Integer) As ActionResult
            Dim notify As Notification = NotificationsController.Instance.GetNotification(notificationId)
            ParseKey(notify.Context)
            Dim cntBlog As New BlogController
            Dim objBlog As BlogInfo = cntBlog.GetBlog(BlogId)

            If objBlog Is Nothing Then
                Return Json(New With {.Result = "error"})
            End If

            If objBlog.AuthorMode = Constants.AuthorMode.PersonalMode Then
                ' this should never happen
                Return Json(New With {.Result = "error"})
            ElseIf objBlog.AuthorMode = Constants.AuthorMode.GhostMode Then
                Dim isOwner As Boolean = objBlog.UserID = UserInfo.UserID

                ' NOTE: we need to allow more than just the owner (think of admin)
                If Not isOwner Then
                    Return Json(New With {.Result = "error"})
                End If

                ' delete the blog post
                Dim cntEntry As New EntryController
                Dim objEntry As EntryInfo = cntEntry.GetEntry(EntryId, PortalSettings.PortalId)

                If objEntry Is Nothing Then
                    Return Json(New With {.Result = "error"})
                End If

                cntEntry.DeleteEntry(EntryId, objEntry.ContentItemId, objBlog.BlogID, PortalSettings.PortalId)
            Else
                ' blogger mode
                Dim isOwner As Boolean = objBlog.UserID = UserInfo.UserID
                Dim objSecurity As New ModuleSecurity(ModuleId, TabId)

                If objSecurity.CanAddEntry(isOwner, Constants.AuthorMode.BloggerMode) Then
                    ' delete the blog post
                    Dim cntEntry As New EntryController
                    Dim objEntry As EntryInfo = cntEntry.GetEntry(EntryId, PortalSettings.PortalId)

                    If objEntry Is Nothing Then
                        Return Json(New With {.Result = "error"})
                    End If

                    cntEntry.DeleteEntry(EntryId, objEntry.ContentItemId, objBlog.BlogID, PortalSettings.PortalId)
                Else
                    Return Json(New With {.Result = "error"})
                End If
            End If
            NotificationsController.Instance().DeleteNotification(notificationId)
            Return Json(New With {.Result = "success"})
        End Function

        <DnnAuthorize()> _
        Public Function IgnorePost(notificationId As Integer) As ActionResult
            Dim notify As Notification = NotificationsController.Instance.GetNotification(notificationId)
            ParseKey(notify.Context)

            Dim cntBlog As New BlogController
            Dim objBlog As BlogInfo = cntBlog.GetBlog(BlogId)

            If objBlog Is Nothing Then
                Return Json(New With {.Result = "error"})
            End If

            NotificationsController.Instance().DeleteNotification(notificationId)
            Return Json(New With {.Result = "success"})

        End Function

#Region "Private Methods"

        Private Sub ParseKey(key As String)
            Dim keys() As String = key.Split(CChar(":"))
            ' 0 is content type string, to ensure unique key
            TabId = Integer.Parse(keys(1))
            ModuleId = Integer.Parse(keys(2))
            BlogId = Integer.Parse(keys(3))
            EntryId = Integer.Parse(keys(4))
        End Sub

#End Region

    End Class

End Namespace