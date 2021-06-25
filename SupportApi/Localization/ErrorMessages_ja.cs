using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SupportApi.Localization
{
    public class ErrorMessages_Ja : ErrorMessages
    {
        public override string CantParseDocumenModifications { get; internal set; } = "エラー: ドキュメントの変更を解析できません。";
        public override string CantParseDocumentOptions { get; internal set; } = "エラー: ドキュメントのオプションを解析できません。";
        public override string DocumentChangesAreNotAllowed { get; internal set; } = "ドキュメントへの変更は許可されていません。";
        public override string EmptyClientMessage { get; internal set; } = "メッセージエラー: 空のメッセージは許可されていません。";
        public override string GcPdfNotLicensedLimit { get; internal set; } = "GcPdfがライセンスなしで使用されているため、読み込みは最大5ページまでに制限されています。";
        public override string InvalidDocumentStructureChanges { get; internal set; } = "無効なドキュメント構造の変更です。";
        public override string MissingDocumentId { get; internal set; } = "必須パラメータ 'documentId' がありませんでした。";
        public override string MissingUserName { get; internal set; } = "必須パラメータ 'userName' がありませんでした。";
        public override string MissingAccessMode { get; internal set; } = "必須パラメータ 'accessMode' がありませんでした。";
        public override string MissingModificationsState { get; internal set; } = "必須のパラメータ 'modificationsState' がありませんでした。";
        public override string NumPagesCheckMismatch { get; internal set; } = "ページ数チェックの不一致です。";
        public override string PersistentConnectionNotFound { get; internal set; } = "持続的なクライアント接続が見つかりません。";
        public override string SharedDocumentNotExists { get; internal set; } = "共有ドキュメントが存在しません。";
        public override string StartErrorFormat { get; internal set; } = "起動エラー: {0}";
        public override string StopErrorFormat { get; internal set; } = "停止エラー: {0}";
        public override string ShareDocumentErrorFormat { get; internal set; } = "ドキュメントの共有エラー: {0}";
        public override string OpenSharedDocumentErrorFormat { get; internal set; } = "共有ドキュメントの読込エラー: {0}";
        public override string Unauthorized { get; internal set; } = "許可がありません。";
        public override string UserNotSharingDocumentFormat { get; internal set; } = "ユーザー {0} はドキュメントへのアクセス権を持っていません。";
        public override string UnshareDocumentErrorFormat { get; internal set; } = "ドキュメント共有の解除エラー: {0}";
        public override string UnknownConnectionIdFormat { get; internal set; } = "不明な接続ID: {0}";
        public override string UnknownChoiceField { get; internal set; } = "不明な選択フィールドです。";
        public override string ModificationErrorFormat { get; internal set; } = "変更エラー: {0}";
        public override string SharedDocumentsListErrorFormat { get; internal set; } = "共有ドキュメント一覧のエラー: {0}";
        public override string SharedDocumentNotFoundFormat { get; internal set; } = "ID {0} の共有ドキュメントが見つかりません。";
        public override string UserAccessListErrorFormat { get; internal set; } = "ユーザーアクセス一覧のエラー: {0}";
        public override string AllUsersListErrorFormat { get; internal set; } = "ユーザー一覧のエラー: {0}";
        public override string ServerErrorFormat { get; internal set; } = "サーバーエラー: {0}";
        public override string ServerErrorDebugFormat { get; internal set; } = "サーバーエラー: {0} コールスタック: {1}";


        public override string CannotAddSharedDocumentToCollectionUnknown { get; internal set; } = "共有ドキュメントをコレクションに追加できません。理由は不明です。";
        public override string CannotAddSharedDocumentToCollectionFormat { get; internal set; } = "共有ドキュメントをコレクションに追加できません。 理由: {0}";
        public override string DocumentAlreadySharedFormat { get; internal set; } = "ID {0} のドキュメントは既に共有されています。";
        public override string UnableToRemoveAnnotationFormat { get; internal set; } = "注釈を削除できません。 理由: {0}";
        public override string UnableToUpdateAnnotationFormat { get; internal set; } = "注釈を更新できません。 理由: {0}";
        public override string UnableToAddAnnotationFormat { get; internal set; } = "注釈を追加できません。 理由: {0}";
        public override string CannotChangeDocumentStructureFormat { get; internal set; } = "ドキュメントの構造を変更できません。 理由: {0}";
        public override string UnableToSaveUndoStateFormat { get; internal set; } = "元に戻した状態を保存できません。 理由: {0}";
        public override string UnableToLoadUndoStateFormat { get; internal set; } = "元に戻した状態から復元できません。 理由: {0}";
        public override string CannotRemoveUserAccessModeFormat { get; internal set; } = "ユーザーのアクセスモードを削除できません。 理由: {0}";
        public override string YouCannotRemoveUserAccess { get; internal set; } = "ユーザーのアクセス権を削除する権限がありません。";
        public override string YouCannotUnshareForOwnerRemoveOthersFirst { get; internal set; } = "所有ユーザーのドキュメントの共有を解除することはできません。まずは他のユーザーを解除してください。";
        public override string YouCannotRestrictAccessMode { get; internal set; } = "アクセスモードを制限する権限がありません。";
        public override string YouCannotRestrictAccessModeForOwner { get; internal set; } = "所有ユーザーのアクセスモードを制限することはできません。";
        public override string DocumentLoaderNotFoundFatal { get; internal set; } = "クライアントがサーバー（ドキュメントローダー）に登録されていません。ページを再読み込みしてください。このエラーが続く場合は、サイト管理者に連絡してください。";
        public override string CreateConnectionMissingConnectionIdInternal { get; internal set; } = "ClientConnectionを作成できませんでした。connectionId が空です。";
        public override string CreateConnectionMissingClientIdInternal { get; internal set; } = "ClientConnectionを作成できませんでした。clientId が空です。";
        public override string NullReferenceExceptionInternal { get; internal set; } = "内部エラー - オブジェクトの参照がオブジェクトのインスタンスに設定されていません。";
        public override string NullReferenceExceptionDebugFormat { get; internal set; } = "内部エラー - オブジェクトの参照がオブジェクトのインスタンスに設定されていません。 詳細: {0} コールスタック: {1}";
        public override string OutOfMemoryExceptionInternal { get; internal set; } = "内部エラー - メモリ不足";
        public override string OutOfMemoryExceptionDebugFormat { get; internal set; } = "内部エラー - メモリ不足 詳細: {0} コールスタック: {1}";
    }
}
