using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SupportApi.Localization
{
    public class ErrorMessages
    {
        public virtual string CantParseDocumenModifications { get; internal set; } = "Error: Cannot parse document modifications.";
        public virtual string CantParseDocumentOptions { get; internal set; } = "Error: Cannot parse document options.";
        public virtual string DocumentChangesAreNotAllowed { get; internal set; } = "Changes to the document are not allowed.";        
        public virtual string EmptyClientMessage { get; internal set; } = "Message error: Empty messages are not allowed.";
        public virtual string GcPdfNotLicensedLimit { get; internal set; } = "GcPdf is not licensed, and can load up to 5 pages.";
        public virtual string InvalidDocumentStructureChanges { get; internal set; } = "Invalid document structure changes.";
        public virtual string MissingDocumentId { get; internal set; } = "A required parameter 'documentId' was missing.";
        public virtual string MissingUserName { get; internal set; } = "A required parameter 'userName' was missing.";
        public virtual string MissingAccessMode { get; internal set; } = "A required parameter 'accessMode' was missing.";
        public virtual string MissingModificationsState { get; internal set; } = "A required parameter 'modificationsState' was missing.";
        public virtual string NumPagesCheckMismatch { get; internal set; } = "Page count check mismatch.";
        public virtual string PersistentConnectionNotFound { get; internal set; } = "Persistent client connection not found.";
        public virtual string SharedDocumentNotExists { get; internal set; } = "The shared document no longer exists.";
        public virtual string StartErrorFormat { get; internal set; } = "Start error: {0}";
        public virtual string StopErrorFormat { get; internal set; } = "Stop error: {0}";
        public virtual string ShareDocumentErrorFormat { get; internal set; } = "ShareDocument error: {0}";
        public virtual string OpenSharedDocumentErrorFormat { get; internal set; } = "OpenSharedDocument error: {0}";
        public virtual string Unauthorized { get; internal set; } = "Unauthorized";
        public virtual string UserNotSharingDocumentFormat { get; internal set; } = "User {0} does not have access to the document.";
        public virtual string UnshareDocumentErrorFormat { get; internal set; } = "UnshareDocument error: {0}";
        public virtual string UnknownConnectionIdFormat { get; internal set; } = "Unknown connection id: {0}";
        public virtual string UnknownChoiceField { get; internal set; } = "Unknown choice field";
        public virtual string ModificationErrorFormat { get; internal set; } = "Modification error: {0}";
        public virtual string SharedDocumentsListErrorFormat { get; internal set; } = "SharedDocumentsList error: {0}";
        public virtual string SharedDocumentNotFoundFormat { get; internal set; } = "Shared document with id {0} is not found.";
        public virtual string UserAccessListErrorFormat { get; internal set; } = "UserAccessList error: {0}";
        public virtual string AllUsersListErrorFormat { get; internal set; } = "AllUsersList error: {0}";
        public virtual string ServerErrorFormat { get; internal set; } = "Server error: {0}";
        public virtual string ServerErrorDebugFormat { get; internal set; } = "Server error: {0} Call stack: {1}";


        public virtual string CannotAddSharedDocumentToCollectionUnknown { get; internal set; } = "Cannot add shared document to collection, unknown reason.";
        public virtual string CannotAddSharedDocumentToCollectionFormat { get; internal set; } = "Cannot add shared document to collection. Reason: {0}";
        public virtual string DocumentAlreadySharedFormat { get; internal set; } = "Document with id {0} already shared.";
        public virtual string UnableToRemoveAnnotationFormat { get; internal set; } = "Unable to remove annotation, reason: {0}";
        public virtual string UnableToUpdateAnnotationFormat { get; internal set; } = "Unable to update annotation, reason {0}";
        public virtual string UnableToAddAnnotationFormat { get; internal set; } = "Unable to add annotation, reason: {0}";
        public virtual string CannotChangeDocumentStructureFormat { get; internal set; } = "Cannot change document structure, reason: {0}";
        public virtual string UnableToSaveUndoStateFormat { get; internal set; } = "Unable to save undo state, reason: {0}";
        public virtual string UnableToLoadUndoStateFormat { get; internal set; } = "Unable to restore from undo state, reason: {0}";
        public virtual string CannotRemoveUserAccessModeFormat { get; internal set; } = "Cannot remove user access mode, reason: {0}";
        public virtual string YouCannotRemoveUserAccess { get; internal set; } = "You do not have permission to remove user access.";
        public virtual string YouCannotUnshareForOwnerRemoveOthersFirst { get; internal set; } = "You cannot unshare the document for the owner user, please remove other users first.";
        public virtual string YouCannotRestrictAccessMode { get; internal set; } = "You do not have permission to restrict access mode.";
        public virtual string YouCannotRestrictAccessModeForOwner { get; internal set; } = "You cannot restrict the access mode of the owner user.";
        public virtual string DocumentLoaderNotFoundFatal { get; internal set; } = "Your client is not registered on server (document loader). Try to reload page. If this error persists, please contact the site administrator.";
        public virtual string CreateConnectionMissingConnectionIdInternal { get; internal set; } = "Cannot create ClientConnection, connectionId is empty.";
        public virtual string CreateConnectionMissingClientIdInternal { get; internal set; } = "Cannot create ClientConnection, clientId is empty.";
        public virtual string NullReferenceExceptionInternal { get; internal set; } = "Internal error - object reference not set to an instance of an object.";
        public virtual string NullReferenceExceptionDebugFormat { get; internal set; } = "Internal error - object reference not set to an instance of an object. Details: {0} Call stack: {1}";        
        public virtual string OutOfMemoryExceptionInternal { get; internal set; } = "Internal error - out of memory.";
        public virtual string OutOfMemoryExceptionDebugFormat { get; internal set; } = "Internal error - out of memory. Details: {0} Call stack: {1}";
    }
}
