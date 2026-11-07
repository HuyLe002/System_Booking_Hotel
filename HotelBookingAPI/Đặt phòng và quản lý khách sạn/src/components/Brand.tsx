import { Link } from "react-router";

export function Brand({
  inverse = false,
  compact = false,
}: {
  inverse?: boolean;
  compact?: boolean;
}) {
  return (
    <Link to="/" className="flex items-center gap-3" aria-label="Aurora Bay - Trang chủ">
      <span
        className={`relative grid size-10 place-items-center rounded-full border ${inverse ? "border-gold/60" : "border-gold"}`}
      >
        <span className={`absolute h-4 w-px rotate-45 ${inverse ? "bg-gold" : "bg-gold-dark"}`} />
        <span className={`absolute h-4 w-px -rotate-45 ${inverse ? "bg-gold" : "bg-gold-dark"}`} />
      </span>
      {!compact && (
        <span>
          <span
            className={`block font-display text-lg font-semibold tracking-[0.16em] ${inverse ? "text-white" : "text-navy"}`}
          >
            AURORA BAY
          </span>
          <span
            className={`block text-[9px] tracking-[0.35em] ${inverse ? "text-white/60" : "text-slate-500"}`}
          >
            HOTEL & RESORT
          </span>
        </span>
      )}
    </Link>
  );
}
