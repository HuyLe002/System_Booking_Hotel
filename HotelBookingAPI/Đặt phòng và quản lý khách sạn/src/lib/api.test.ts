import { describe, expect, it } from "vitest";
import { normalizePaged, queryString } from "./api";

describe("normalizePaged", () => {
  it("normalizes a plain array", () => {
    expect(normalizePaged([{ id: 1 }])).toEqual({
      items: [{ id: 1 }],
      pageNumber: 1,
      pageSize: 1,
      totalCount: 1,
      totalPages: 1,
    });
  });

  it("normalizes PagedResult fields", () => {
    expect(normalizePaged({
      items: [{ id: 1 }, { id: 2 }],
      pageNumber: 3,
      pageSize: 2,
      totalCount: 6,
      totalPages: 3,
    })).toMatchObject({
      pageNumber: 3,
      pageSize: 2,
      totalCount: 6,
      totalPages: 3,
    });
  });
});

describe("queryString", () => {
  it("maps pageNumber to page and ignores empty values", () => {
    expect(queryString({
      pageNumber: 2,
      pageSize: 10,
      search: "",
      status: undefined,
    })).toBe("?page=2&pageSize=10");
  });
});
