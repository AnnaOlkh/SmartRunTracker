interface ConfirmDialogProps {
    title: string;
    message: string;
    confirmLabel?: string;
    cancelLabel?: string;
    onConfirm: () => void;
    onCancel: () => void;
  }
  
  export function ConfirmDialog({
    title,
    message,
    confirmLabel = "Delete",
    cancelLabel = "Cancel",
    onConfirm,
    onCancel,
  }: ConfirmDialogProps) {
    return (
      <div className="dialog-backdrop" role="presentation">
        <section className="confirm-dialog" role="dialog" aria-modal="true">
          <h3>{title}</h3>
          <p className="muted">{message}</p>
  
          <div className="actions">
            <button type="button" className="danger-button" onClick={onConfirm}>
              {confirmLabel}
            </button>
  
            <button type="button" className="secondary-button" onClick={onCancel}>
              {cancelLabel}
            </button>
          </div>
        </section>
      </div>
    );
  }